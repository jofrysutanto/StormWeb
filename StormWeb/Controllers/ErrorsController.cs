using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Helper;

namespace StormWeb.Controllers
{
    public class ErrorsController : Controller
    {
        //
        // GET: /Errors/

        public ActionResult Error404()
        {
            return View();
        }

        /// <summary>
        /// Redirect user to the home page with error message.
        /// By default, "You clicked on bad links" message will be shown unless error message is passed to this action
        /// </summary>
        /// <param name="message">Can be omitted</param>
        /// <returns></returns>
        public ActionResult BadLink(string message = null)
        {
            if (message == null)
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "You clicked on bad links");
            else
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, message);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
