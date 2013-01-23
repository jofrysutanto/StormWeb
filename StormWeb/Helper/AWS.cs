using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;

namespace StormWeb.Helper
{
    public class AWS
    {
        // upload to Amazon  s3
        //[HttpPost]
        //public static ActionResult uploadAWS(string path, HttpPostedFileBase file)
        //{

        //    if (!checkRequiredFields())
        //    {
        //        ViewBag.Result = "";
        //        return View();
        //    }
        //    AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.APSoutheast1);

        //    PutObjectRequest request = new PutObjectRequest();

        //    request.BucketName = bucketName;
        //    request.ContentType = ("applicaton/pdf");
        //    request.Key = path;
        //    request.StorageClass = S3StorageClass.ReducedRedundancy; //set storage to reduced redundancy
        //    request.InputStream = file.InputStream;

        //    try
        //    {
        //        S3Response uploadResponse = client.PutObject(request);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.InnerException);
        //    }

        //    return View("Refresh");
        //}

        //[HttpPost]
        //public static ActionResult DeletingAWS(string keyName)
        //{
        //    if (!checkRequiredFields())
        //    {
        //        ViewBag.Result = "";
        //        return View();
        //    }
        //    try
        //    {

        //        DeleteObjectRequest request = new DeleteObjectRequest();
        //        request.WithBucketName(bucketName)
        //            .WithKey(keyName);
        //        AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.APSoutheast1);
        //        DeleteObjectResponse response = client.DeleteObject(request);

        //        //using (DeleteObjectResponse response = client.DeleteObject(request))
        //        //{
        //        //    WebHeaderCollection headers = response.Headers;
        //        //    foreach (string key in headers.Keys)
        //        //    {
        //        //        Console.WriteLine("Response Header: {0}, Value: {1}", key, headers.Get(key));
        //        //    }
        //        //}
        //    }
        //    catch (AmazonS3Exception amazonS3Exception)
        //    {
        //        if (amazonS3Exception.ErrorCode != null &&
        //            (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
        //            amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
        //        {
        //            Console.WriteLine("Please check the provided AWS Credentials.");
        //            Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
        //        }
        //        else
        //        {
        //            Console.WriteLine("An error occurred with the message '{0}' when deleting an object", amazonS3Exception.Message);
        //        }

        //        NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error removing the file from our File System, this file record is now removed");
        //    }
        //    return View("Refresh");
        //}


        //public static void downloadAWS(int id, string path, string filename)
        //{
        //    string keyName = path + "/" + filename;

        //    if (!checkRequiredFields())
        //    {
        //        ViewBag.Result = "";

        //    }
        //    AmazonS3 client;
        //    try
        //    {
        //        using (client = Amazon.AWSClientFactory.CreateAmazonS3Client())
        //        {
        //            GetObjectRequest request = new GetObjectRequest().WithBucketName(bucketName).WithKey(keyName);

        //            //string dest = ("C:\\user\\Downloads\\" + path + "\\" + filename);

        //            string dest = HttpContext.Server.MapPath("~/App_Data/Downloads/" + id + '-' + filename);
        //            using (GetObjectResponse response = client.GetObject(request))
        //            {
        //                response.WriteResponseStreamToFile(dest, false);

        //                HttpContext.Response.Clear();
        //                HttpContext.Response.AppendHeader("content-disposition", "attachment; filename=" + filename);
        //                HttpContext.Response.ContentType = response.ContentType;
        //                HttpContext.Response.TransmitFile(dest);
        //                HttpContext.Response.Flush();
        //                HttpContext.Response.End();
        //            }


        //            //Clean up temporary file.
        //            //System.IO.File.Delete(dest);

        //        }
        //    }
        //    catch (AmazonS3Exception amazonS3Exception)
        //    {
        //        if (amazonS3Exception.ErrorCode != null &&
        //            (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
        //            amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
        //        {
        //            Console.WriteLine("Please check the provided AWS Credentials.");
        //            Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
        //        }
        //        else
        //        {
        //            Console.WriteLine("An error occurred with the message '{0}' when reading an object", amazonS3Exception.Message);
        //        }


        //    }

        //}
    }
}