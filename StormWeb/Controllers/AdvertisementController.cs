using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;


namespace StormWeb.Controllers
{ 
    public class AdvertisementController : Controller
    {
        public static string MARKETING_UPLOADS_PATH = "Marketing";
        private StormDBEntities db = new StormDBEntities();

        private string accessKey = "";
        private string secretKey = "";
        private string bucketName = "";

        //
        // GET: /Advertisement/

        public ViewResult Index()
        {
            return View(db.Advertisements.ToList());
        }

        //
        // GET: /Advertisement/Details/5

        public ViewResult Details(int id)
        {
            AdvertisementViewModel viewModel = new AdvertisementViewModel();
            viewModel.adv = db.Advertisements.SingleOrDefault(a => a.AdvertisementId == id);
            viewModel.adv_file = db.Advertisement_File.Single(a => a.AdvertisementId == id);

            return View(viewModel);
            //Advertisement advertisement = db.Advertisements.Single(a => a.AdvertisementId == id);
            //return View(advertisement);
        }

        //
        // GET: /Advertisement/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Advertisement/Create

        public void AdvertisementFile(int id)
        {
            Advertisement_File appDoc = null;

            appDoc = db.Advertisement_File.SingleOrDefault(x => x.AdvertisementFileId == id);

            if (appDoc == null)
                return;
            downloadAWS(appDoc.AdvertisementFileId, appDoc.Path, appDoc.FileName);
            //string path = appDoc.Path;
            //string fileToDownload = appDoc.FileName;
            //string file = Path.Combine(path, fileToDownload);
            //string ext = Path.GetExtension(file);
            //string contentType = "application/doc/pdf";

            ////Parameters to file are
            ////1. The File Path on the File Server
            ////2. The content type MIME type
            ////3. The parameter for the file save by the browser

            //return File(file, contentType, appDoc.FileName);
        } 

        [HttpPost]
        public ActionResult Create(AdvertisementViewModel advertisement, HttpPostedFileBase filename)
        {
           

            Advertisement adv = advertisement.adv;
            Advertisement_File adv_file = new Advertisement_File();
            

            if (filename != null)
            {
                //String name = Path.GetFileNameWithoutExtension(filename.FileName);
                string pathToCreate;
                pathToCreate = MARKETING_UPLOADS_PATH;
                string fileToCreate = pathToCreate;
                adv_file.FileName = DateTime.Now.ToString("MMddyy")+DateTime.Now.ToString("hms")+ "_" + filename.FileName;
                adv_file.Path = fileToCreate;

                uploadAWS(fileToCreate + '/' + adv_file.FileName, filename);
            }

            
            adv_file.UploadedOn = DateTime.Now;

            adv_file.UploadedBy = @CookieHelper.Name;

  

            adv.Advertisement_File.Add(adv_file);

            if (ModelState.IsValid)
            {
                db.Advertisements.AddObject(adv);
                db.SaveChanges();

                return RedirectToAction("Index", "Advertisement");
            }
            else
            {
                return View(advertisement);
            }
        }

       
        
        //
        // GET: /Advertisement/Edit/5
 
        public ActionResult Edit(int id)
        {       
            AdvertisementViewModel viewModel = new AdvertisementViewModel();

            viewModel.adv = db.Advertisements.SingleOrDefault(a => a.AdvertisementId == id);
            viewModel.adv_file =  db.Advertisement_File.Single(a => a.AdvertisementId == id);

            return View(viewModel);
        }

        //
        // POST: /Advertisement/Edit/5

        [HttpPost]
        public ActionResult Edit(AdvertisementViewModel advertisement)
        {
            Advertisement adv = db.Advertisements.Single(x => x.AdvertisementId == advertisement.adv.AdvertisementId);
            Advertisement_File adv_file = db.Advertisement_File.Single(x => x.AdvertisementId == adv.AdvertisementId);

            adv.Comments = advertisement.adv.Comments;
            adv.Heading = advertisement.adv.Heading;
           

            //adv = advertisement.adv;
            //adv_file = advertisement.adv_file;

            //db.SaveChanges();

            if (ModelState.IsValid)
            {
                //db.Advertisements.Attach(advertisement.adv);
                //db.Advertisement_File.Attach(advertisement.adv_file);
                //db.ObjectStateManager.ChangeObjectState(advertisement.adv, EntityState.Modified);
                //db.ObjectStateManager.ChangeObjectState(advertisement.adv_file, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(advertisement);
        }

        //
        // GET: /Advertisement/Delete/5
 
        public ActionResult Delete(int id)
        {
            AdvertisementViewModel viewModel = new AdvertisementViewModel();

            viewModel.adv = db.Advertisements.SingleOrDefault(a => a.AdvertisementId == id);
            viewModel.adv_file = db.Advertisement_File.Single(a => a.AdvertisementId == id);

            return View(viewModel);

            //return View(viewModel);
            //Advertisement advertisement = db.Advertisements.Single(a => a.AdvertisementId == id);
            //return View(advertisement);
        }

        //
        // POST: /Advertisement/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Advertisement adv = db.Advertisements.Single(x => x.AdvertisementId == id);
            Advertisement_File adv_file = db.Advertisement_File.Single(x => x.AdvertisementId == id);
            //Advertisement advertisement = db.Advertisements.Single(a => a.AdvertisementId == id);
            db.Advertisements.DeleteObject(adv);
            db.Advertisement_File.DeleteObject(adv_file);
            db.SaveChanges();
            DeletingAWS(adv_file.Path + '/' + adv_file.FileName);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #region AWS
        public bool checkRequiredFields()
        {
            ServiceConfiguration config = new ServiceConfiguration();

            accessKey = config.AWSAccessKey;
            secretKey = config.AWSSecretKey;
            bucketName = config.BucketName;

            if (string.IsNullOrEmpty(accessKey))
            {
                Console.WriteLine("AWSAccessKey was not set in the App.config file.");
                return false;
            }
            if (string.IsNullOrEmpty(secretKey))
            {
                Console.WriteLine("AWSSecretKey was not set in the App.config file.");
                return false;
            }
            if (string.IsNullOrEmpty(bucketName))
            {
                Console.WriteLine("The variable bucketName is not set.");
                return false;
            }

            return true;
        }

        public ViewResult viewAWS()
        {
            string result = "";
            if (!checkRequiredFields())
            {
                ViewBag.Result = "";
                return View();
            }
            AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.APSoutheast1);

            ListObjectsRequest request = new ListObjectsRequest();


            request.BucketName = bucketName;
            using (ListObjectsResponse response = client.ListObjects(request))
            {
                foreach (S3Object entry in response.S3Objects)
                {
                    result += "key = " + entry.Key + " size = " + entry.Size + "<br/>";
                }
            }
            ViewBag.Result = result;
            return View();
        }


        //ViewBag.Result = result;
        //return View();


        //string result = "";

        //try
        //{

        //    ListObjectsRequest request = new ListObjectsRequest();
        //    request.BucketName = bucketName;

        //    PutObjectRequest uploadrequest = new PutObjectRequest();
        //    uploadrequest.WithContentBody("this is a test")
        //        .WithBucketName(bucketName)
        //        .WithKey("testupload.pdf")
        //        .WithContentType("applicaton/pdf")

        //        //.WithFilePath("Upload/2_Stewie")
        //        .WithInputStream(file.InputStream);


        //    S3Response uploadResponse = client.PutObject(uploadrequest);
        //    uploadResponse.Dispose();
        //}
        //catch (AmazonS3Exception amazonS3Exception)
        //{
        //    if (amazonS3Exception.ErrorCode != null && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
        //    {
        //        Console.WriteLine("Please check the provided AWS Credentials.");
        //        Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
        //    }
        //    else
        //    {
        //        Console.WriteLine("An error occurred with the message '{0}' when listing objects", amazonS3Exception.Message);
        //    }
        //}

        //using (ListObjectsResponse response = client.ListObjects(request))
        //{
        //    foreach (S3Object entry in response.S3Objects)
        //    {
        //        result += "key = " + entry.Key + " size = " + entry.Size + "<br/>";
        //    }
        //}

        // simple object put


        //ViewBag.Result = result;
        //return View();



        // upload to Amazon  s3
        [HttpPost]
        public ActionResult uploadAWS(string path, HttpPostedFileBase file)
        {

            if (!checkRequiredFields())
            {
                ViewBag.Result = "";
                return View();
            }
            AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.APSoutheast1);

            PutObjectRequest request = new PutObjectRequest();

            request.BucketName = bucketName;
            request.ContentType = ("applicaton/pdf");
            request.Key = path;
            request.StorageClass = S3StorageClass.ReducedRedundancy; //set storage to reduced redundancy
            request.InputStream = file.InputStream;

            try
            {
                S3Response uploadResponse = client.PutObject(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }

            return View("Refresh");

        }

        [HttpPost]

        public ActionResult DeletingAWS(string keyName)
        {
            if (!checkRequiredFields())
            {
                ViewBag.Result = "";
                return View();
            }
            try
            {

                DeleteObjectRequest request = new DeleteObjectRequest();
                request.WithBucketName(bucketName)
                    .WithKey(keyName);
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.APSoutheast1);
                DeleteObjectResponse response = client.DeleteObject(request);

                //using (DeleteObjectResponse response = client.DeleteObject(request))
                //{
                //    WebHeaderCollection headers = response.Headers;
                //    foreach (string key in headers.Keys)
                //    {
                //        Console.WriteLine("Response Header: {0}, Value: {1}", key, headers.Get(key));
                //    }
                //}
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when deleting an object", amazonS3Exception.Message);
                }

                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error removing the file from our File System, this file record is now removed");
            }
            return View("Refresh");
        }


        public void downloadAWS(int id, string path, string filename)
        {
            string keyName = path + "/" + filename;

            if (!checkRequiredFields())
            {
                ViewBag.Result = "";

            }
            AmazonS3 client;
            try
            {
                using (client = Amazon.AWSClientFactory.CreateAmazonS3Client())
                {
                    GetObjectRequest request = new GetObjectRequest().WithBucketName(bucketName).WithKey(keyName);

                    //string dest = ("C:\\user\\Downloads\\" + path + "\\" + filename);

                    string dest = HttpContext.Server.MapPath("~/App_Data/Downloads/" + id + '-' + filename);
                    using (GetObjectResponse response = client.GetObject(request))
                    {
                        response.WriteResponseStreamToFile(dest, false);

                        HttpContext.Response.Clear();
                        HttpContext.Response.AppendHeader("content-disposition", "attachment; filename=" + filename);
                        HttpContext.Response.ContentType = response.ContentType;
                        HttpContext.Response.TransmitFile(dest);
                        HttpContext.Response.Flush();
                        HttpContext.Response.End();
                    }


                    //Clean up temporary file.
                    //System.IO.File.Delete(dest);

                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when reading an object", amazonS3Exception.Message);
                }


            }

        }
        #endregion

    }

}