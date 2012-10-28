using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StormWeb.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /Help/
        [Authorize(Roles="Student")]
        public ActionResult Index()
        {
            return View();
        }

    }
}
