using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StormWeb.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "Counsellor,Student,Super,BranchManager")]
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            if (StormWeb.Helper.CookieHelper.isStudent())
                return RedirectToAction("Student", "StudentCentre");
            else if (StormWeb.Helper.CookieHelper.isStaff())
                return RedirectToAction("Index", "StaffCentre");
            else
                return RedirectToAction("LogOn", "Account");
        }
        [Authorize(Roles = "Counsellor,Student,Super,BranchManager")]
        public ActionResult About()
        {
            return View();
        }
    }
}
