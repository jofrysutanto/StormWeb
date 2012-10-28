/* Author: Jofry HS
 * Date: 20/09/2012
 * 
 * Application Controller
 * 
 * Handles all applications opnened for the student * 
 * 
 * */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;

namespace StormWeb.Controllers
{ 
    public class ApplicationController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Application/
        [Authorize(Roles="Counsellor,Student")]
        public ActionResult Index(int id = -1)
        {
            // If accessing application without specifying the Student ID
            if (id == -1)
            {
                // If the current user is the student show his/her application
                if (CookieHelper.isStudent())

                {
                    id = CookieHelper.getStudentId();
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // If staff, validate if the student actually assigned to the staff
            if (CookieHelper.isStaff())
            {
                if (!StudentsHelper.staffAssignedToStudent(CookieHelper.getStaffId(), id))
                    return RedirectToAction("Index", "Home");

            }
           
            
            var applications = db.Applications.Where(x => x.Student_Id == id);
            return View(applications.ToList());
        }

        //
        // GET: /Application/Details/5
        [Authorize(Roles="Counsellor,Student")]
        public ViewResult Details(int id)
        {
            Application application = db.Applications.Single(a => a.Application_Id == id);
            return View(application);
        }

        //
        // GET: /Application/Create
        [Authorize(Roles="Student")]
        public ActionResult Create()
        {
            ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy");
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name");
            //ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Course_Choice");
            ViewBag.Student_Id=new SelectList(db.Students,"Student_Id","Student_Id");
            return View();
        }

        [Authorize(Roles = "Student,Counsellor")]
        [HttpGet]
        public ActionResult Add(int id, int sid)
        {
            Application app = new Application();

            Application test = db.Applications.DefaultIfEmpty(null).SingleOrDefault(x => x.Student_Id == sid && x.Course_Id == id);
            if (test != null)
            {
                TempData["message"] = "error";
                return RedirectToAction("Index", "Home");
            }

            app.Case_Id = db.Cases.SingleOrDefault(x => x.Student_Id == sid).Case_Id;
            app.Student_Id = sid;
            app.Date_Of_ApplicationStatus = DateTime.Now;
            app.Course_Id = id;
            app.Status = Enum.GetNames(typeof(ApplicationStatusType))[0];

            db.Applications.AddObject(app);
            db.SaveChanges();

            TempData["message"] = "success";
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "You have successfully started an application");
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Application/Create
        [Authorize(Roles = "Student")]
        [HttpPost]
        public ActionResult Create(Application application)
        {
            if (ModelState.IsValid)
            {
                db.Applications.AddObject(application);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy", application.Case_Id);
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name", application.Course_Id);
           // ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Course_Choice", application.Student_Id);
            ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Student_Id",application.Student.Client.GivenName);
            return View(application);
        }
        
        //
        // GET: /Application/Edit/5
        [Authorize(Roles = "Student")]
        public ActionResult Edit(int id)
        {
            Application application = db.Applications.Single(a => a.Application_Id == id);
            ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy", application.Case_Id);
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name", application.Course_Id);
           // ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Course_Choice", application.Student_Id);
            ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Student_Id", application.Student.Client.GivenName);
            return View(application);
        }

        //
        // POST: /Application/Edit/5
        [Authorize(Roles = "Student")]
        [HttpPost]
        public ActionResult Edit(Application application)
        {
            if (ModelState.IsValid)
            {
                db.Applications.Attach(application);
                db.ObjectStateManager.ChangeObjectState(application, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy", application.Case_Id);
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name", application.Course_Id);
           // ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Course_Choice", application.Student_Id);
            ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Student_Id", application.Student.Client.GivenName);
            return View(application);
        }

        //
        // GET: /Application/Delete/5
        [Authorize(Roles = "Student")]
        public ActionResult Delete(int id)
        {
            Application application = db.Applications.Single(a => a.Application_Id == id);
            
            return View(application);
        }

        //
        // POST: /Application/Delete/5
        [Authorize(Roles = "Student,Counsellor")]
        //[HttpPost]
        public ActionResult DeleteConfirmed(int id)
        {
            var appDoc = db.Application_Document.Where(x => x.Application_Id == id);
            Application application = db.Applications.Single(a => a.Application_Id == id);
            if (appDoc.Count() > 0)
            {
                List<Application_Document> aDoc = appDoc.ToList();
                
                foreach (Application_Document item in aDoc)
                {
                    if (StormWeb.Helper.CookieHelper.isStaff())
                    {
                        int staffId = Convert.ToInt32(CookieHelper.StaffId);
                        String staffName = db.Staffs.Single(x => x.Staff_Id == staffId).FirstName;
                        LogHelper.writeToLog(application.Case_Id, staffName + " deleted an application document uploaded by " + application.Student.Client.GivenName, LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);
                    }
                    else
                    {
                        int studentId = Convert.ToInt32(CookieHelper.StudentId);
                        LogHelper.writeToLog(application.Case_Id, application.Student.Client.GivenName + " deleted and application document", LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);
                    }
                    
                    db.Application_Document.DeleteObject(item);
                    db.SaveChanges();
                }
                    
            }
            if(CookieHelper.isStaff())
            {
                int staffId = Convert.ToInt32(CookieHelper.StaffId);
                String staffName = db.Staffs.Single(x => x.Staff_Id == staffId).FirstName;
                LogHelper.writeToLog(application.Case_Id, staffName + " deleted the application created by " + application.Student.Client.GivenName, LogHelper.LOG_DELETE, LogHelper.SECTION_APPLICATION);
            }
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Application Deleted");
            db.Applications.DeleteObject(application);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
            
        }

        [HttpPost]
        public JsonResult setStatus(int studentId, int courseId, int status)
        {
            Application application = db.Applications.Single(a => a.Student_Id == studentId && a.Course_Id == courseId);
            JsonResult result = new JsonResult();

            string newStatus = Enum.GetName(typeof(ApplicationStatusType), status);
            application.Status = newStatus;

            db.SaveChanges();

            result.Data = true;

            return result;
        }

        
        public static void setStatus(int applicat, int status)
        {
            StormDBEntities db = new StormDBEntities();

            Application application = db.Applications.Single(x => x.Application_Id == applicat);

            string newStatus = Enum.GetName(typeof(ApplicationStatusType), status);
            application.Status = newStatus;

            db.SaveChanges();

            return;
        }

        public static int getProgressValue(string status)
        {
            int value = (int) Enum.Parse(typeof(ApplicationStatusType), status);

            return value;
        }

        public static string getProgressDescription(string status)
        {
            ApplicationStatusType type = (ApplicationStatusType) Enum.Parse(typeof(ApplicationStatusType), status);

            switch (type)
            {
                case ApplicationStatusType.Initiated:
                    return "Initiated";
                case ApplicationStatusType.Staff_Assigned:
                    return "Staff has been assigned";
                case ApplicationStatusType.Documents_Completed:
                    return "Documents are uploaded";
                case ApplicationStatusType.Application_Submitted:
                    return "Application is submitted to university";
                case ApplicationStatusType.Offer_Letter:
                    return "Offer letter is issued";
                case ApplicationStatusType.CoE:
                    return "Confirmation of Enrollment is issued";
                default:
                    return "Not recognized!";
            }
        }

        public enum ApplicationStatusType
        {
            Initiated = 0,
            Staff_Assigned = 20,
            Documents_Completed = 40,
            Application_Submitted = 60,
            Offer_Letter = 80,
            CoE = 100
        }

        public ActionResult changeStatus(int id)
        {
            Application application = db.Applications.Single(x=>x.Application_Id==id);
            application.Status = ApplicationController.ApplicationStatusType.Application_Submitted.ToString();
            application.Date_Of_ApplicationStatus = DateTime.Now;
            db.ObjectStateManager.ChangeObjectState(application, EntityState.Modified);
            db.SaveChanges();
            LogHelper.writeToLog(new string[] { CookieHelper.Username }, (" Changed the application status of the Student : " + application.Student.Client.GivenName +" "+ application.Student.Client.LastName + "and Application Id : " + id), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
            return RedirectToAction("Index");
        }

        public ActionResult markStatusCompleted(int id)
        {
            Application application = db.Applications.Single(x => x.Application_Id == id); 
            application.Completed = true;
            db.ObjectStateManager.ChangeObjectState(application, EntityState.Modified);
            db.SaveChanges();
            LogHelper.writeToLog(new string[] { CookieHelper.Username }, (" Changed the application status of the Student: " +application.Student.Client.GivenName +" "+ application.Student.Client.LastName + "and Application Id : " + id), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
            return RedirectToAction("Index");
        }
        public JsonResult CancelApplication(string comment, int id)
        {
            Application application = db.Applications.Single(x => x.Application_Id == id);
            Application_Cancel application_cancel = new Application_Cancel();
            application_cancel = new Application_Cancel();
            application_cancel.Status = true;
            application_cancel.Comment = comment;
            application_cancel.Application_Id = application.Application_Id;
            db.Application_Cancel.AddObject(application_cancel);
            db.SaveChanges();
            LogHelper.writeToLog(new string[] { CookieHelper.Username }, ("Requested a cancelation for the application :" + application.Course.Course_Name + "by the Student : " + application.Student.Client.GivenName + " " + application.Student.Client.LastName), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
            return Json(new {Success=true}, JsonRequestBehavior.AllowGet); 
        }

         private static string mycomment = "";
    }
}