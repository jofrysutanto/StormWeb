using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Models.ModelHelper;
using StormWeb.Helper;
using Newtonsoft.Json;


namespace StormWeb.Controllers
{ 
    public class EventController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Event/

        public ViewResult Index()
        {
           
            //ViewBag.eventNow = db.Events.ToList(); 
            return View(db.Events.ToList());
        }

       
        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Event/Create

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Event event1,FormCollection formEvent)
        {
            //Event events = new Event();
            // events.Date= DateTime.Now;
            event1.Date = DateTime.Now;
            event1.EventAddedBy = Convert.ToInt32(formEvent["EventAddedBy"]);
            if (ModelState.IsValid)
            {
                db.Events.AddObject(event1);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View();
        }

        
 
        public ActionResult Edit(int id)
        {
            Event event1 = db.Events.Single(e => e.EventId == id);
            return View(event1);
        }

        //
        // POST: /Event/Edit/5

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Event event1)
        {
            if (ModelState.IsValid)
            {
                db.Events.Attach(event1);
                db.ObjectStateManager.ChangeObjectState(event1, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(event1);
        }

        //
        // GET: /Event/Delete/5
 
        public ActionResult Delete(int id)
        {
            Event event1 = db.Events.Single(e => e.EventId == id);
            return View(event1);
        }

        //
        // POST: /Event/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Event event1 = db.Events.Single(e => e.EventId == id);
            db.Events.DeleteObject(event1);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //[AcceptVerbs(HttpVerbs.Get)]
        //public JsonResult GetEvents(double start, double end)
        //{
        //    var events = new List<CalendarEvent>();
        //    var dtstart = ConvertFromUnixTimestamp(start);
        //    var dtend = ConvertFromUnixTimestamp(end);
        //    int sId = 0;
            
        //    if(CookieHelper.isStudent())
        //    {
        //        sId = Convert.ToInt32(CookieHelper.StudentId);
                
        //        Case checkCase = (from c in db.Cases
        //                         where c.Student_Id == sId
        //                         select c).Single();
        //        if(checkCase != null)
        //        {
        //            var onCalls =from c in db.Appointments
        //                      where c.Case_Id == checkCase.Case_Id && c.AppDateTime > DateTime.Now && c.Confirmation == AppointmentController.APP_CONFIRMED 
        //                      select new {c.AppDateTime,c.Case.Student.Client.GivenName,c.Case.Student.Student_Id,c.Staff.FirstName};
        //            DateTime currStart;
        //            DateTime currEnd;
        //            foreach (var p in onCalls)
        //            {
        //                currStart = Convert.ToDateTime(p.AppDateTime);
        //                currEnd = Convert.ToDateTime(p.AppDateTime);
        //                events.Add(new CalendarEvent()
        //                {
        //                    id = p.Student_Id.ToString(),
        //                    title = "Appointment with "+p.FirstName,
        //                    start = currStart.ToString(),
        //                    end = currEnd.ToString(),
        //                    url = "/Event/Details/" + p.Student_Id.ToString() +"/",
        //                    className = "stud",
        //                    allDay = false
        //                });
        //            }
        //        }
        //        else
        //        {
        //            var onCalls = from p in db.Appointments
        //                       from g in db.General_Enquiry
        //                       from c in db.Clients
        //                       where p.Appointment_Id == g.Appointment_Id && c.Client_Id == g.Client_Id && p.Confirmation == AppointmentController.APP_CONFIRMED && p.AppDateTime > DateTime.Now
        //                       select new { p.AppDateTime, p.General_Enquiry.SingleOrDefault().Client.GivenName,p.General_Enquiry.SingleOrDefault().Client.Client_Id ,p.Staff.FirstName};
        //            DateTime currStart;
        //            DateTime currEnd;
        //            foreach (var p in onCalls)
        //            {
        //                currStart = Convert.ToDateTime(p.AppDateTime);
        //                currEnd = Convert.ToDateTime(p.AppDateTime);
        //                events.Add(new CalendarEvent()
        //                {
        //                    id = p.Client_Id.ToString(),
        //                    title = "Appointment with "+p.FirstName,
        //                    start = currStart.ToString(),
        //                    end = currEnd.ToString(),
        //                    url = "/Event/Details/" + p.Client_Id.ToString() + "/",
        //                    className = "genStud",
        //                    allDay = false
        //                });
        //            }
        //        }
        //    }
        //    else
        //    {
        //        sId = Convert.ToInt32(CookieHelper.StaffId);
        //        var onCalls = (from p in db.Appointments
        //              where p.Staff_Id == sId && p.Confirmation == AppointmentController.APP_CONFIRMED && p.AppDateTime > DateTime.Now
        //              select new{p.AppDateTime,p.Staff.FirstName,p.Staff_Id,p.Case.Student.Client.GivenName,p.Appointment_Id});
        //        //var onCall = db.Events.Single(e => e.EventAddedBy == sId);
        //        DateTime currStart;
        //        DateTime currEnd;
        //        foreach (var p in onCalls)
        //        {
        //            currStart = Convert.ToDateTime(p.AppDateTime);
        //            currEnd = currStart.AddHours(1);
        //            var studName = "";
        //            if (p.GivenName == null)
        //            {
        //                studName = (from a in db.Appointments
        //                            from g in db.General_Enquiry
        //                            where g.Appointment_Id == a.Appointment_Id && a.Appointment_Id == p.Appointment_Id
        //                            select g.Client.GivenName).Single();

        //            }
        //            else
        //            {
        //                studName = p.GivenName;
        //            }
        //            var combStudStaff = p.Staff_Id.ToString() + "," + p.Appointment_Id;
        //            events.Add(new CalendarEvent()
        //            {
        //                id = p.Staff_Id.ToString(),
        //                title = "Appointment with " + studName,
        //                start = currStart.ToString(),
        //                end = currEnd.ToString(),
        //                url = "/Event/Details/" + combStudStaff + "/",
        //                className = "staff",
        //                allDay = false
        //            });
        //        }
        //    }

            
        //    var rows = events.ToArray();
        //    return Json(rows, JsonRequestBehavior.AllowGet);
        //}

        //private static DateTime ConvertFromUnixTimestamp(double timestamp)
        //{
        //    var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        //    return origin.AddSeconds(timestamp);
        //}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}