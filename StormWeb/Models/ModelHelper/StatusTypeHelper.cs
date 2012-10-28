using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models.ModelHelper
{
    public class StatusTypeHelper
    {
        public string StatusType { get; set; }

        public static IQueryable<StatusTypeHelper> GetStatus()
        {
            return new List<StatusTypeHelper>  
            {  
                new StatusTypeHelper {  
                    StatusType = "Student Visa Holder"
                },  
                new StatusTypeHelper {  
                    StatusType = "Tourist/Visitor Visa Holder"
                },  
                new StatusTypeHelper {  
                    StatusType = "Temporary Resident"
                },  
                new StatusTypeHelper {  
                    StatusType = "Permanent Resident"
                },  
                new StatusTypeHelper {  
                    StatusType = "Citizen"
                },
                new StatusTypeHelper {  
                    StatusType = "Others"
                }
            }.AsQueryable();
        }    
    }
}