using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.S3.Transfer;

namespace StormWeb.Helper
{
    public class S3Uploader
    {
        private string awsAccessKeyId;
        private string awsSecretAccessKey;
        private string bucketName;

       
       
        private Amazon.S3.Transfer.TransferUtility transferUtility;

        public S3Uploader(string bucketName)
        {
            this.bucketName = bucketName;
            this.transferUtility = new Amazon.S3.Transfer.TransferUtility(awsAccessKeyId, awsSecretAccessKey);

        }

        public void UploadFile(string filePath, string toPath)
        {
            AsyncCallback callback = new AsyncCallback(uploadComplete);
            var uploadRequest = new TransferUtilityUploadRequest();
            uploadRequest.FilePath = filePath;
            uploadRequest.BucketName = bucketName;
            uploadRequest.Key = toPath;
            
            transferUtility.BeginUpload(uploadRequest, callback, null);
        }

        private void uploadComplete(IAsyncResult result)
        {
            var x = result;
        }
    }
}