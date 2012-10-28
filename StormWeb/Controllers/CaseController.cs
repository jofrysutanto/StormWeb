using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StormWeb.Controllers
{
    public class CaseController : Controller
    {
        //
        // GET: /Case/
        [Authorize(Roles="Counsellor")]
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Case/Details/5

        [Authorize(Roles = "Counsellor")]
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Case/Create

        [Authorize(Roles = "Counsellor")]
        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Case/Create

        [Authorize(Roles = "Counsellor")]
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /Case/Edit/5
[Authorize(Roles = "Counsellor")]
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Case/Edit/5

        [Authorize(Roles = "Counsellor")]
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Case/Delete/5

        [Authorize(Roles = "Counsellor")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Case/Delete/5

        [Authorize(Roles = "Counsellor")]
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        #region Case Status Code

        public static string CASE_INITIATED = "Initiated";

        #endregion
    }
}
