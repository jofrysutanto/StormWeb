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
    public class MaterialController : Controller
    {
        public static string MATERIAL_UPLOADS_PATH = "Marketing/Material";

        public static string ORDER_STATUS_PENDING = "Pending";
        public static string ORDER_STATUS_APPROVED = "Approved";
        public static string ORDER_STATUS_COMPLETED = "Completed";

        private StormDBEntities db = new StormDBEntities();

        private string accessKey = "";
        private string secretKey = "";
        private string bucketName = "";

        //
        // GET: /Material/

        public ViewResult Index()
        {
            MaterialViewModel viewModel = new MaterialViewModel();

            viewModel.material = db.Materials.ToList();

            if (CookieHelper.isInRole("Marketing"))
            {
                viewModel.placeOrder = db.Material_Order.ToList();
            }
            else
            {
                int branchID = (BranchHelper.getBranchIDArray(CookieHelper.AssignedBranch))[0];
                viewModel.placeOrder = db.Material_Order.Where(x => x.Branch == branchID).ToList();
            }

            return View(viewModel);
        }

        //
        // GET: /Material/Details/5

        public ViewResult Details(int id)
        {
            Material material = db.Materials.Single(m => m.ID == id);
            return View(material);
        }

        //
        // GET: /Material/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Material/Create

        [HttpPost]
        public ActionResult Create(Material material,HttpPostedFileBase filename)
        {
            if (filename != null)
            {
                //String name = Path.GetFileNameWithoutExtension(filename.FileName);
                string pathToCreate;
                pathToCreate = MATERIAL_UPLOADS_PATH;
                string fileToCreate = pathToCreate;

                Material_File mf = new Material_File();

                mf.Filename = DateTime.Now.ToString("MMddyy") + DateTime.Now.ToString("hms") + "_" + filename.FileName;
                mf.Path = fileToCreate;
                mf.UploadedBy = CookieHelper.Username;
                mf.UploadedOn = DateTime.Now;

                material.Material_File.Add(mf);

                uploadAWS(fileToCreate + '/' + mf.Filename, filename);

               
            }
            else if(material.HasFile && filename == null)
            {
                ModelState.AddModelError("NoFile", "You have to upload a file.");
            }

            if (ModelState.IsValid)
            {
                db.Materials.AddObject(material);
                db.SaveChanges();

               
                return RedirectToAction("Index");  
            }

            return View(material);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AddInstore(int id, int q)
        {
            Material m = db.Materials.SingleOrDefault(x => x.ID == id);

            if (id != null)
            {
                if (q != null && q > 0)
                {
                    m.Instore = m.Instore + q;
                    db.SaveChanges();

                    Material_Order ord = new Material_Order();
                    ord.Branch = BranchHelper.getBranchIDArray(CookieHelper.AssignedBranch)[0];
                    ord.Material_ID = m.ID;
                    ord.Quantity = q;
                    ord.RequestedBy = "Marketing";
                    ord.RequestedOn = DateTime.Now;
                    ord.Type = "Add";
                    ord.Status = "Completed";

                    db.AddToMaterial_Order(ord);
                    db.SaveChanges();

                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Material stock updated");
                }
                else
                {
                    NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error processing your request, make sure you netered valid amount");
                }
            }

            return RedirectToAction("Index");
        }
        
        public ActionResult CreateOrder(int id)
        {
            ViewBag.MaterialQuantity = db.Materials.Single(x => x.ID == id).Instore;
            return View();
        }

        //
        // POST: /Material/Create

        [HttpPost]
        public ActionResult CreateOrder(Material_Order order,int id)
        {
            order.Material_ID = id;

            int instore = db.Materials.Single(x => x.ID == id).Instore;

            if (order.Quantity > instore)
                ModelState.AddModelError("Quantity", "Can't order more than instore");

           if (ModelState.IsValid)
            {
                db.Material_Order.AddObject(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

           ViewBag.MaterialQuantity = db.Materials.Single(x => x.ID == id).Instore;

            return View(order);
        }

        public void MaterialFile(int id)
        {
            Material_File mfDoc = null;

            mfDoc = db.Material_File.SingleOrDefault(x => x.ID == id);

            if (mfDoc == null)
                return;
            downloadAWS(mfDoc.ID, mfDoc.Path, mfDoc.Filename);
            
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

        
        //
        // GET: /Material/Edit/5
 
        public ActionResult Edit(int id)
        {
            Material material = db.Materials.Single(m => m.ID == id);
            return View(material);
        }

        //
        // POST: /Material/Edit/5

        [HttpPost]
        public ActionResult Edit(Material material)
        {
            if (ModelState.IsValid)
            {
                db.Materials.Attach(material);
                db.ObjectStateManager.ChangeObjectState(material, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(material);
        }

        public ActionResult EditOrder(int id)
        {
            Material_Order order = db.Material_Order.Single(m => m.ID == id);
            return View(order);
        }

        //
        // POST: /Material/Edit/5

        [HttpPost]
        public ActionResult EditOrder(Material_Order order)
        {
            if (ModelState.IsValid)
            {
                db.Material_Order.Attach(order);
                db.ObjectStateManager.ChangeObjectState(order, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(order);
        }
        //
        // GET: /Material/Delete/5
 
        public ActionResult Delete(int id)
        {
            Material material = db.Materials.Single(m => m.ID == id);
            return View(material);
        }

        //
        // POST: /Material/Delete/5

       [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Material material = db.Materials.Single(m => m.ID == id);

           // Checking and delete Material_file
           if (db.Material_File.SingleOrDefault(x => x.Material_ID == id) != null)
            {
                Material_File file = db.Material_File.Single(x => x.Material_ID == id);
                db.Material_File.DeleteObject(file);
            }
           // End of Material_File

           // Checking and delete all Material_Order(s)
            List<Material_Order> orders = db.Material_Order.Where(x => x.Material_ID == id).ToList();
            foreach (Material_Order ord in orders)
            {
                db.Material_Order.DeleteObject(ord);
            }
           // End of Material_Order(s)

           // Delete Material
            db.Materials.DeleteObject(material);
           // End of Material

           // Finalise deletion
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteOrder(int id)
        {
           // Checking and delete Material_order
            if (db.Material_Order.SingleOrDefault(x => x.ID == id) != null)
            {
                Material_Order ordered = db.Material_Order.Single(x => x.ID == id);
                return View(ordered);
            }
            // End of Material_order
           return View("Index");
        }

        //
        // POST: /Material/Delete/5

        [HttpPost, ActionName("DeleteOrder")]
        public ActionResult OrderDelete(int id)
        {
            Material material = db.Materials.Single(m => m.ID == id);
            Material_File file = db.Material_File.Single(x => x.Material_ID == id);
            Material_Order order = db.Material_Order.Single(m => m.ID == id);
            db.Material_Order.DeleteObject(order);
            db.Material_File.DeleteObject(file);
            db.Materials.DeleteObject(material);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ApproveOrder(int id)
        {
            Material_Order order = db.Material_Order.Single(m => m.ID == id);
            if (order.Status.Contains("Pending"))
            {
                order.Status = "Approved";
            }
            if (order.Status.Contains("Approved"))
            {
                order.Status = "Completed";
                order.CompletedOn = DateTime.Now;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

       
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}