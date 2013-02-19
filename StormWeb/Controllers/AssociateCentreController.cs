using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;

namespace StormWeb.Controllers
{
    public class AssociateCentreController : Controller
    {
        private StormDBEntities db = new StormDBEntities();
        //
        // GET: /AssociateCentre/

        public ActionResult Index()
        {
            ViewBag.eve = db.Events.Where(x => x.Date > DateTime.Now).ToList();
            ViewBag.Ads = db.Advertisements.Where(x => x.ExpiryDate > DateTime.Now).ToList();
            int AssociateId = Convert.ToInt32(CookieHelper.AssocId);
            var clients = db.Clients.Where(x => x.Associate_Id == AssociateId).ToList();
            return View(clients.ToList());
        }

    }
}
