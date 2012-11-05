using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Helper
{
    public class PaymentHelper
    {
        public string MethodType { get; set; }

        public static IQueryable<PaymentHelper> GetType()
        {
            return new List<PaymentHelper>  
            {  
                new PaymentHelper {  
                    MethodType = "--Choose one--"
                },  
                new PaymentHelper {  
                    MethodType = "Credit Card"
                },  
                new PaymentHelper {  
                    MethodType = "Cash"
                }
             }.AsQueryable();
        }
    }



}