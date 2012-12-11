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
        [Authorize(Roles = "Counsellor,Administrator,Student")]
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
        [Authorize(Roles = "Counsellor,Administrator,Student")]
        public ViewResult Details(int id)
        {
            Application application = db.Applications.Single(a => a.Application_Id == id);
            return View(application);
        }

        //
        // GET: /Application/Create
        [Authorize(Roles = "Student")]
        public ActionResult Create()
        {
            ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy");
            ViewBag.Course_Id = new SelectList(db.Courses, "Course_Id", "Course_Name");
            //ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Course_Choice");
            ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Student_Id");
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
            app.Completed = false;

            app.Interview_Institution = false;
            app.Interview_Storm = false;

            db.Applications.AddObject(app);
            db.SaveChanges();

            TempData["message"] = "success";
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "You have successfully started an application");
            return RedirectToAction("Index", "Document", new { go = app.Application_Id, faq = true });
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
            ViewBag.Student_Id = new SelectList(db.Students, "Student_Id", "Student_Id", application.Student.Client.GivenName);
            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Name + " started a New Application to the" + application.Course.Faculty.University.University_Name + "university for the courses " + application.Course.Course_Name), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
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
            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Name + " edited the application regarding to " + application.Course.Faculty.University.University_Name + "university for the courses " + application.Course.Course_Name), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
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
            Application application = db.Applications.SingleOrDefault(a => a.Application_Id == id);
            Application_Cancel applicationCancel = db.Application_Cancel.SingleOrDefault(x => x.Application_Id == id);
            if (appDoc.Count() > 0)
            {
                List<Application_Document> aDoc = appDoc.ToList();

                foreach (Application_Document item in aDoc)
                {
                    if (StormWeb.Helper.CookieHelper.isStaff())
                    {
                        int staffId = Convert.ToInt32(CookieHelper.StaffId);
                        String staffName = db.Staffs.Single(x => x.Staff_Id == staffId).FirstName;
                        LogHelper.writeToStudentLog(application.Case_Id, staffName + " deleted an application document uploaded by " + application.Student.Client.GivenName, LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);
                    }
                    else
                    {
                        int studentId = Convert.ToInt32(CookieHelper.StudentId);
                        LogHelper.writeToStudentLog(application.Case_Id, application.Student.Client.GivenName + " deleted and application document", LogHelper.LOG_DELETE, LogHelper.SECTION_DOCUMENT);
                    }

                    db.Application_Document.DeleteObject(item);
                    db.SaveChanges();
                }

            }
            if (CookieHelper.isStaff())
            {
                int staffId = Convert.ToInt32(CookieHelper.StaffId);
                String staffName = db.Staffs.Single(x => x.Staff_Id == staffId).FirstName;
                LogHelper.writeToStudentLog(application.Case_Id, staffName + " deleted the application created by " + application.Student.Client.GivenName, LogHelper.LOG_DELETE, LogHelper.SECTION_APPLICATION);
            }
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Application Deleted");
            if (applicationCancel != null)
                db.Application_Cancel.DeleteObject(applicationCancel);
            db.Applications.DeleteObject(application);
            db.SaveChanges();
            //LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Name + " deleted the application regarding to " + application.Course.Faculty.University.University_Name + "university for the courses " + application.Course.Course_Name), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);
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
            int value = (int)Enum.Parse(typeof(ApplicationStatusType), status);

            return value;
        }

        public static string getStaffName(int applicationId)
        {
            StormDBEntities db = new StormDBEntities();


            int caseId = db.Applications.SingleOrDefault(x => x.Application_Id == applicationId).Case_Id;
            var staffId = db.Case_Staff.SingleOrDefault(x => x.Case_Id == caseId).Staff_Id;
            var staff = db.Staffs.SingleOrDefault(x => x.Staff_Id == staffId);

            string staffName = staff.FirstName + " " + staff.LastName;
            return staffName;
        }

        public static string getProgressDescription(string status)
        {
            ApplicationStatusType type = (ApplicationStatusType)Enum.Parse(typeof(ApplicationStatusType), status);

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
                case ApplicationStatusType.Payment_Received:
                    return "Payment is received";
                case ApplicationStatusType.CoE:
                    return "Confirmation of Enrollment is issued";
                default:
                    return "Not recognized!";
            }
        }

        public ActionResult changeStatus(int id)
        {
            Application application = db.Applications.Single(x => x.Application_Id == id);
            application.Status = ApplicationController.ApplicationStatusType.Application_Submitted.ToString();
            //ApplicationController.statusUp(application.Application_Id);
            application.Date_Of_ApplicationStatus = DateTime.Now;
            db.ObjectStateManager.ChangeObjectState(application, EntityState.Modified);
            db.SaveChanges();

            string sys_message = "Your application for " + application.Course.Course_Name + " at " + application.Course.Faculty.University.University_Name + " has been submitted.";

            MessageController.sendSystemMessage(application.Student.UserName, "Application submission", sys_message);

            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Changed the application status of the Student : " + application.Student.Client.GivenName + " " + application.Student.Client.LastName + "for the Application : " + application.Course.Course_Name), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
            return RedirectToAction("StudentDetails", "Document", new { id=application.Student.Client_Id });
        }

        public ActionResult updateInterview(int id, string type, string back = "")
        {
            Application ap = db.Applications.SingleOrDefault(x => x.Application_Id == id);
            if (ap == null)
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error processing request: application does not exist!");
                return Redirect(RouteHelper.handleReturnURL(back));
            }
            if (type == "storm")
            {
                if (ap.Interview_Storm)
                    ap.Interview_Storm = false;
                else
                    ap.Interview_Storm = true;

                db.SaveChanges();
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Interview storm successfully updated");

                if (ap.Interview_Storm && ap.Interview_Institution)
                    statusUp(ap.Application_Id);

                return Redirect(RouteHelper.handleReturnURL(back));
            }
            else if (type == "institution")
            {
                if (ap.Interview_Institution)
                    ap.Interview_Institution = false;
                else
                    ap.Interview_Institution = true;

                db.SaveChanges();

                if (ap.Interview_Storm && ap.Interview_Institution)
                    statusUp(ap.Application_Id);

                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Interview institution successfully updated");
                return Redirect(RouteHelper.handleReturnURL(back));
            }
            else
            {
                NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Error processing request: 'type of interview' not specified");
                return Redirect(RouteHelper.handleReturnURL(back));
            }
        }

        [HttpPost]
        public ActionResult UpdatePayment(int appID, string amount, string currency)
        {
            Payment pay = db.Payments.SingleOrDefault(x => x.Application_Id == appID);

            if (pay == null)
                return Json(new { data = "false" });

            pay.Amount = Convert.ToDecimal(amount);
            pay.Currency = currency;

            MessageController.sendSystemMessage(pay.Application.Case.Student.UserName, "Payment updated", "The payment information for you application: " + pay.Application.Course.Course_Name + " have been updated.");

            db.SaveChanges();

            return Json(new { data="true" });           
        }

        [HttpPost]
        public ActionResult getAmountPayable(int appID)
        {
            Payment pay = db.Payments.SingleOrDefault(x => x.Application_Id == appID);

            if (pay == null)
                return Json(new { data = "false" });

            decimal amount = (decimal) pay.Amount;
            string currency = pay.Currency;

            return Json(new { amount = amount, currency = currency });
        }

        public ActionResult markStatusCompleted(int id)
        {
            Application application = db.Applications.Single(x => x.Application_Id == id);
            application.Completed = true;
            db.ObjectStateManager.ChangeObjectState(application, EntityState.Modified);
            db.SaveChanges();
            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Changed the application status of the Student: " + application.Student.Client.GivenName + " " + application.Student.Client.LastName + "for the Application : " + application.Course.Course_Name), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
            return RedirectToAction("Index");
        }
        public JsonResult CancelApplicationJSON(string comment, int id)
        {
            Application application = db.Applications.Single(x => x.Application_Id == id);
            Application_Cancel application_cancel = new Application_Cancel();
            application_cancel = new Application_Cancel();
            application_cancel.Status = true;
            application_cancel.Comment = comment;
            application_cancel.Application_Id = application.Application_Id;
            db.Application_Cancel.AddObject(application_cancel);
            db.SaveChanges();
            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Requested a cancelation for the application :" + application.Course.Course_Name + "by the Student : " + application.Student.Client.GivenName + " " + application.Student.Client.LastName), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CancelApplication(string comment, int id)
        {
            Application application = db.Applications.Single(x => x.Application_Id == id);
            Application_Cancel application_cancel = new Application_Cancel();
            application_cancel = new Application_Cancel();
            application_cancel.Status = true;
            application_cancel.Comment = comment;
            application_cancel.Application_Id = application.Application_Id;
            db.Application_Cancel.AddObject(application_cancel);
            db.SaveChanges();
            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Requested a cancelation for the application :" + application.Course.Course_Name + "by the Student : " + application.Student.Client.GivenName + " " + application.Student.Client.LastName), LogHelper.LOG_UPDATE, LogHelper.SECTION_PROFILE);
            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);            
        }

        public static void deleteAllApplications(int applicationId)
        {
            StormDBEntities db = new StormDBEntities();
           
            var applicationsToDelete = db.Applications.Where(x => x.Application_Id == applicationId);

            var applicationsCancelToDelete = db.Application_Cancel.Where(x => x.Application_Id == applicationId);

            var applicationsDocumentToDelete = db.Application_Document.Where(x => x.Application_Id == applicationId);

            var applicationsResultToDelete = db.Application_Result.Where(x => x.Application_Id == applicationId);

            if (applicationsToDelete.Count() > 0)
            {
                foreach (Application applications in applicationsToDelete)
                {
                    db.Applications.DeleteObject(applications);
                }
            }
            if (applicationsCancelToDelete.Count() > 0)
            {
                foreach (Application_Cancel applicationsCancel in applicationsCancelToDelete)
                {
                    db.Application_Cancel.DeleteObject(applicationsCancel);
                }
            }
            if (applicationsDocumentToDelete.Count() > 0)
            {
                foreach (Application_Document applicationsDocuments in applicationsDocumentToDelete)
                {
                    db.Application_Document.DeleteObject(applicationsDocuments);
                }
            }
            if (applicationsResultToDelete.Count() > 0)
            {
                foreach (Application_Result applicationsResult in applicationsResultToDelete)
                {
                    db.Application_Result.DeleteObject(applicationsResult);
                }
            }  
        }

        public static void statusUp(int applicationID)
        {
            StormDBEntities db = new StormDBEntities();
            Application app = db.Applications.SingleOrDefault(x => x.Application_Id == applicationID);
            if (app == null)
                return;

            bool found = false;
            foreach(var statusType in Enum.GetValues(typeof(ApplicationStatusType)))
            {
                if (found)
                {
                    app.Status = statusType.ToString();
                    break;
                }
                if (getProgressValue(app.Status) == (int)statusType)
                {
                    found = true;
                }                
            }

            db.SaveChanges();
        }

        public static void statusDown(int applicationID)
        {
            StormDBEntities db = new StormDBEntities();
            Application app = db.Applications.SingleOrDefault(x => x.Application_Id == applicationID);
            if (app == null)
                return;

            bool found = false;

            string[] names = Enum.GetNames(typeof(ApplicationStatusType));
            ApplicationStatusType[] values = (ApplicationStatusType[])Enum.GetValues(typeof(ApplicationStatusType));

            for (int i = names.Length-1; i >= 0; i--)
            {
                if (found)
                {
                    app.Status = names[i];
                    break;
                }
                if (getProgressValue(app.Status) == (int)values[i])
                {
                    found = true;
                } 
            }

            db.SaveChanges();
        }

        public enum ApplicationStatusType
        {
            Initiated = 0,
            Staff_Assigned = 20,
            Documents_Completed = 40,
            Application_Submitted = 60,
            Interview_Completed = 70,
            Offer_Letter = 80,
            Payment_Received = 85,
            Acceptance = 90,
            CoE = 100
        }

        public static int getApplicationStatusTypeValue(string status)
        {
            return (int)Enum.Parse(typeof(StormWeb.Controllers.ApplicationController.ApplicationStatusType), status);
        }
        public static bool requestedApplicationCancel(int applicationId)
        {
            StormDBEntities db = new StormDBEntities();
            bool status = false;
            var applicationCancel = db.Application_Cancel.SingleOrDefault(x => x.Application_Id == applicationId);
            if (applicationCancel != null)
                status = applicationCancel.Status;
            return status;
        }

        public static string getNextStepDescription(string status)
        {
            ApplicationStatusType currentStat = (ApplicationStatusType) Enum.Parse(typeof(StormWeb.Controllers.ApplicationController.ApplicationStatusType), status);

            switch (currentStat)
            {
                case ApplicationStatusType.Initiated:
                    return "This application just started. Please approve all the documents uploaded by the student.";
                case ApplicationStatusType.Staff_Assigned:
                    return "Waiting for students to upload all required documents. You have to approve all documents uploaded by the students.";
                case ApplicationStatusType.Documents_Completed:
                    return "Application is ready for submission";
                case ApplicationStatusType.Application_Submitted:
                    return "Waiting for storm and universities interview completion";
                case ApplicationStatusType.Interview_Completed:
                    return "Waiting for university to issue the offer letter. ";
                case ApplicationStatusType.Offer_Letter:
                    return "Please update the fees payment details for student and wait for student payment.";
                case ApplicationStatusType.Payment_Received:
                    return "Upload the acceptance form and wait for student acceptance form.";
                case ApplicationStatusType.Acceptance:
                    return "Waiting for confirmation of enrolment";                
                case ApplicationStatusType.CoE:
                    return "Application is complete";
                default:
                    return "Unknown application status";
            }
        }

        internal static void deleteApplication(int? nullable)
        {
            throw new NotImplementedException();
        }

        internal static void deleteApplications(int? nullable)
        {
            throw new NotImplementedException();
        }
    }
    
    //public static class ApplicationStatusType
    //{
    //    public static int Initiated = 0;
    //    public static int Staff_Assigned = 20;
    //    public static int Documents_Completed = 40;
    //    public static int Application_Submitted = 60;
    //    public static int Interview_Completed = 70;
    //    public static int Offer_Letter = 80;
    //    public static int Payment_Received = 90;
    //    public static int CoE = 100;

    //public static class ApplicationStatusType
    //{
    //    public static int Initiated = 0;
    //    public static int Staff_Assigned = 20;
    //    public static int Documents_Completed = 40;
    //    public static int Application_Submitted = 60;
    //    public static int Interview_Completed = 70;
    //    public static int Offer_Letter = 80;
    //    public static int Payment_Received = 90;
    //    public static int CoE = 100;

    //    public static int[] Value = { 0, 20, 40, 60, 80, 90, 100 };
    //    public static string[] Name = { "Initiated", "Staff_Assigned", "Documents_Completed", "Offer_Letter", "Payment_Received", "CoE" };


    //}
}