using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StormWeb.Helper
{
    public class RouteHelper
    {
        public static string defaultURL = "/Home/Index";

        public static string handleReturnURL(string back)
        {
            if (back == "")
            {
                return defaultURL;
            }
            else
                return back;
        }
    }
}