using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models.ModelHelper
{
    public class ServiceHelper
    {
        public string ServiceType { get; set; }

        public static IQueryable<ServiceHelper> GetItems()
        {
            return new List<ServiceHelper>  
            {  
                new ServiceHelper {  
                    ServiceType = "--Choose one--"
                },  
                new ServiceHelper {  
                    ServiceType = "Study"
                },  
                new ServiceHelper {  
                    ServiceType = "Migration"
                }
            }.AsQueryable();
        }               
    }
}