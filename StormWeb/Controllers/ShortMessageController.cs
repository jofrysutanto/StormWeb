using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Helper;
using Clockwork;
using System.Net;

namespace StormWeb.Controllers
{
    public class ShortMessageController : Controller
    {
        //
        // GET: /ShortMessage/
        ServiceConfiguration config = new ServiceConfiguration();

        string accessKey =  new ServiceConfiguration().Clockwork;

        public ActionResult Index()
        {

            try
            {
                Clockwork.API api = new API(accessKey);
                SMSResult result = api.Send(new SMS { To = "61449912603", Message = "Hello World" });

                if (result.Success)
                {
                    Console.WriteLine("SMS Sent to {0}, Clockwork ID: {1}", result.SMS.To, result.ID);
                }
                else
                {
                    Console.WriteLine("SMS to {0} failed, Clockwork Error: {1} {2}", result.SMS.To, result.ErrorCode, result.ErrorMessage);
                }
            }
            catch (APIException ex)
            {
                // You'll get an API exception for errors
                // such as wrong username or password
                Console.WriteLine("API Exception: " + ex.Message);
            }
            catch (WebException ex)
            {
                // Web exceptions mean you couldn't reach the Clockwork server
                Console.WriteLine("Web Exception: " + ex.Message);
            }
            catch (ArgumentException ex)
            {
                // Argument exceptions are thrown for missing parameters,
                // such as forgetting to set the username
                Console.WriteLine("Argument Exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Something else went wrong, the error message should help
                Console.WriteLine("Unknown Exception: " + ex.Message);
            }
            return View();
        }

    }
}
