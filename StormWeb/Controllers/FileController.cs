using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StormWeb.Controllers
{
    public class FileController : Controller
    {
        //
        // GET: /File/
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

    }
}
