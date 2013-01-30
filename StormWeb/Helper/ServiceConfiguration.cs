using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Helper
{
    public class ServiceConfiguration
    {
        public ServiceConfiguration()
        {
            this.ServerEmail = System.Configuration.ConfigurationManager.AppSettings["ServerEmail"];
            this.EmailPassword = System.Configuration.ConfigurationManager.AppSettings["EmailPassword"];
            this.AWSAccessKey = System.Configuration.ConfigurationManager.AppSettings["AWSAccessKey"];
            this.AWSSecretKey = System.Configuration.ConfigurationManager.AppSettings["AWSSecretKey"];
            this.BucketName = System.Configuration.ConfigurationManager.AppSettings["BucketName"];
            this.Clockwork = System.Configuration.ConfigurationManager.AppSettings["Clockwork"];
        }

        public string ServerEmail { get; private set; }
        public string EmailPassword { get; private set; }
        public string AWSAccessKey { get; private set; }
        public string AWSSecretKey { get; private set; }
        public string BucketName { get; private set; }
        public string Clockwork { get; private set; }
    }
}