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
        }

        public string ServerEmail { get; private set; }
        public string EmailPassword { get; private set; }
    }
}