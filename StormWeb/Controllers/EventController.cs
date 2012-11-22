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
            return View(new EventAppointment());
        }

        //
        // GET: /Event/Details/5

        public ViewResult Details(string id)
        {
            //Event event1 = db.Events.Single(e => e.EventId == id);
            EventAppointment appEvent = new EventAppointment();
            string[] temp = id.Split(',');
            int staffId = Convert.ToInt32(temp[0]);
            int appointmentId = Convert.ToInt32(temp[1]);
            appEvent.appointment = db.Appointments.Single(a => a.Appointment_Id == appointmentId);
            if (appEvent.appointment.Case_Id == null)
            {
                appEvent.gen = db.General_Enquiry.Single(a => a.Appointment_Id == appointmentId);
                appEvent.client = db.Clients.Single(x => x.Client_Id == appEvent.gen.Client_Id);
            }
            else
            {
                appEvent.cases = db.Cases.Single(c => c.Case_Id == appEvent.appointment.Case_Id);
                appEvent.student = db.Students.Single(s => s.Student_Id == appEvent.cases.Student_Id);
                appEvent.client = db.Clients.Single(s => s.Client_Id == appEvent.student.Client_Id);
            }
            appEvent.staff = db.Staffs.Single(s => s.Staff_Id == staffId);
            appEvent.address = db.Addresses.Single(s => s.Address_Id == appEvent.staff.Address_Id);
            appEvent.add = appEvent.address.Address_Name + "," + appEvent.address.City + "\n" + appEvent.address.State + "," + appEvent.address.Country.Country_Name + "-" + appEvent.address.Zipcode;
            return View(appEvent); 

        }

        //
        // GET: /Event/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Event/Create

        /*[HttpGet]
        public ActionResult Create(Event event1,FormCollection formEvent)
        {
            string titles = Request.QueryString["title"];
            if (ModelState.IsValid)
            {
                db.Events.AddObject(event1);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(event1);
        }*/

        [HttpPost]
        public JsonResult Create(string eventTitle, string eventStart, string eventEnd, string eventStartTime, string eventEndTime, string eventVenue, string eventComment)
        {
            Event events = new Event();
            if(CookieHelper.isStaff())
            {
                int staffId = Convert.ToInt32(CookieHelper.StaffId);
                events.EventAddedBy = staffId;
                events.StartDate = DateTime.ParseExact(eventStart, "d", null);
                events.EndDate = DateTime.ParseExact(eventEnd, "d", null);
                if (eventStartTime != "" && eventEndTime != "")
                {
                    events.StartTime = eventStartTime;
                    events.EndTime = eventEndTime;
                }
                events.Venue = eventVenue;
                events.Comments = eventComment;

                if (ModelState.IsValid)
                {
                    db.Events.AddObject(events);
                    db.SaveChanges();

                }

            }
            return Json(eventTitle);
        }
        //
        // GET: /Event/Edit/5
 
        public ActionResult Edit(int id)
        {
            Event event1 = db.Events.Single(e => e.EventId == id);
            return View(event1);
        }

        //
        // POST: /Event/Edit/5

        [HttpPost]
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

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetEvents(double start, double end)
        {
            var events = new List<CalendarEvent>();
            var dtstart = ConvertFromUnixTimestamp(start);
            var dtend = ConvertFromUnixTimestamp(end);
            int sId = 0;
            
            if(CookieHelper.isStudent())
            {
                sId = Convert.ToInt32(CookieHelper.StudentId);
                
                Case checkCase = (from c in db.Cases
                                 where c.Student_Id == sId
                                 select c).Single();
                if(checkCase != null)
                {
                    var onCalls =from c in db.Appointments
                              where c.Case_Id == checkCase.Case_Id && c.AppDateTime > DateTime.Now && c.Confirmation == AppointmentController.APP_CONFIRMED 
                              select new {c.AppDateTime,c.Case.Student.Client.GivenName,c.Case.Student.Student_Id,c.Staff.FirstName};
                    DateTime currStart;
                    DateTime currEnd;
                    foreach (var p in onCalls)
                    {
                        currStart = Convert.ToDateTime(p.AppDateTime);
                        currEnd = Convert.ToDateTime(p.AppDateTime);
                        events.Add(new CalendarEvent()
                        {
                            id = p.Student_Id.ToString(),
                            title = "Appointment with "+p.FirstName,
                            start = currStart.ToString(),
                            end = currEnd.ToString(),
                            url = "/Event/Details/" + p.Student_Id.ToString() +"/",
                            className = "stud",
                            allDay = false
                        });
                    }
                }
                else
                {
                    var onCalls = from p in db.Appointments
                               from g in db.General_Enquiry
                               from c in db.Clients
                               where p.Appointment_Id == g.Appointment_Id && c.Client_Id == g.Client_Id && p.Confirmation == AppointmentController.APP_CONFIRMED && p.AppDateTime > DateTime.Now
                               select new { p.AppDateTime, p.General_Enquiry.SingleOrDefault().Client.GivenName,p.General_Enquiry.SingleOrDefault().Client.Client_Id ,p.Staff.FirstName};
                    DateTime currStart;
                    DateTime currEnd;
                    foreach (var p in onCalls)
                    {
                        currStart = Convert.ToDateTime(p.AppDateTime);
                        currEnd = Convert.ToDateTime(p.AppDateTime);
                        events.Add(new CalendarEvent()
                        {
                            id = p.Client_Id.ToString(),
                            title = "Appointment with "+p.FirstName,
                            start = currStart.ToString(),
                            end = currEnd.ToString(),
                            url = "/Event/Details/" + p.Client_Id.ToString() + "/",
                            className = "genStud",
                            allDay = false
                        });
                    }
                }
            }
            else
            {
                sId = Convert.ToInt32(CookieHelper.StaffId);
                var onCalls = (from p in db.Appointments
                      where p.Staff_Id == sId && p.Confirmation == AppointmentController.APP_CONFIRMED && p.AppDateTime > DateTime.Now
                      select new{p.AppDateTime,p.Staff.FirstName,p.Staff_Id,p.Case.Student.Client.GivenName,p.Appointment_Id});
                //var onCall = db.Events.Single(e => e.EventAddedBy == sId);
                DateTime currStart;
                DateTime currEnd;
                foreach (var p in onCalls)
                {
                    currStart = Convert.ToDateTime(p.AppDateTime);
                    currEnd = currStart.AddHours(1);
                    var studName = "";
                    if (p.GivenName == null)
                    {
                        studName = (from a in db.Appointments
                                    from g in db.General_Enquiry
                                    where g.Appointment_Id == a.Appointment_Id && a.Appointment_Id == p.Appointment_Id
                                    select g.Client.GivenName).Single();

                    }
                    else
                    {
                        studName = p.GivenName;
                    }
                    var combStudStaff = p.Staff_Id.ToString() + "," + p.Appointment_Id;
                    events.Add(new CalendarEvent()
                    {
                        id = p.Staff_Id.ToString(),
                        title = "Appointment with " + studName,
                        start = currStart.ToString(),
                        end = currEnd.ToString(),
                        url = "/Event/Details/" + combStudStaff + "/",
                        className = "staff",
                        allDay = false
                    });
                }
            }

            
            var rows = events.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        private static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}