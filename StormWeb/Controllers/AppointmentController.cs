// --------------------------------------------------------------------------------------------------------------------
// <summary>
// File Name    : AppointmentController.cs
// Created Date : 14/08/2012
// Created By   : Deane Liz Joseph
// Description  : All actions related to appointments applicable to both counselor and student.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Models.ModelHelper;                                                                                                                                               
using System.Diagnostics;
using System.Web.Security;
using System.Globalization;
using System.Web.UI;
using StormWeb.Helper;

namespace StormWeb.Controllers
{

    public class AppointmentController : Controller
    {
        #region Confirmation Status

        public static string APP_REQUEST_APPROVAL = "Request for Approval";
        public static string APP_CONFIRMED = "Confirmed";
        public static string APP_ATTENDED = "Attended";
        public static string APP_NOT_ATTENDED = "Not Attended";

        #endregion

        private StormDBEntities db = new StormDBEntities();

        #region FirstUpcomingAppointment
        public static Appointment FirstUpcomingAppointment()
        {
            StormDBEntities db = new StormDBEntities();
            Appointment appointment = null;
            DateTime current = DateTime.Now;
            if (StormWeb.Helper.CookieHelper.isStudent())
            {
                int studentId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StudentId);
                Case cases = db.Cases.DefaultIfEmpty(null).SingleOrDefault(x => x.Student_Id == studentId);

                if (cases == null)
                    return null;

                var a = from appo in db.Appointments
                               where appo.Case_Id == cases.Case_Id && appo.Confirmation == APP_CONFIRMED && appo.AppDateTime >= current
                               orderby appo.AppDateTime ascending
                               select appo;
                if (a.Count() > 0)
                    appointment = a.First();

            }
            else
            {

                int staffId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StaffId);
                var a = from app in db.Appointments
                               where app.Confirmation == APP_CONFIRMED && app.Staff_Id == staffId && app.AppDateTime >= current
                               orderby app.AppDateTime ascending
                               select app;
                if (a.Count() > 0)
                    appointment = a.First();

            }
            return appointment;
        }
        #endregion

        #region Index
        [Authorize(Roles="Student,Counsellor")]
        [AcceptVerbs(HttpVerbs.Get)]
         public ViewResult Index()
        {
            StaffAppointmentListViewModel model = new StaffAppointmentListViewModel();
            var appointments = db.Appointments.Include("Case");
            if (StormWeb.Helper.CookieHelper.isStudent())
            {
                int studentId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StudentId);
                /* *
                 * Getting branch address
                 * */
                String branchName = StormWeb.Helper.BranchHelper.getBranchListFromCookie().First().Branch_Name;
                Address add = (from addr in db.Addresses
                              from br in db.Branches
                              where br.Address_Id == addr.Address_Id && br.Branch_Name == branchName
                              select addr).Single();
                String address = add.Address_Name + "," + add.City + "\n" + add.State + "," + add.Country.Country_Name + "-" + add.Zipcode;
                ViewBag.BranchAddress = address;
                /* *
                 * Gets the list of appointment made by the student who have a Case ID
                 * */
                Case cases = db.Cases.Single(x => x.Student_Id == studentId);
                DateTime current = DateTime.Now;
                var app = db.Appointments.ToList().Where(x => (x.Case_Id == cases.Case_Id) && (x.Confirmation == APP_CONFIRMED || x.Confirmation == APP_REQUEST_APPROVAL) && x.AppDateTime>DateTime.Now);
                model.studentAppointments = app.ToList();

                /* *
                 * Get the previous appointments made by the student
                 * */
                var previousAppointments = db.Appointments.ToList().Where(x => ((x.Confirmation == APP_ATTENDED) || (x.Confirmation == APP_NOT_ATTENDED)) && (x.Case_Id == cases.Case_Id) && (x.AppDateTime < current));
                model.studentPreviousApp = previousAppointments.ToList();

                /* *
                 * Gets the details of the counsellor staff asssigned to the particular counsellor
                 * */
                Staff staff = (from cs in db.Case_Staff
                               from s in db.Staffs
                               where cs.Case_Id == cases.Case_Id && cs.Staff_Id == s.Staff_Id && cs.Role.Equals("Counsellor")
                               select s).SingleOrDefault();
                if (staff == null)
                {
                    ViewBag.StaffName = "No counsellor has been assigned";
                    ViewBag.StaffContactNumber = "";
                    ViewBag.StaffEmail = "";
                }
                else
                {
                    ViewBag.StaffName = staff.FirstName + " " + staff.LastName;
                    ViewBag.StaffContactNumber = staff.Mobile_Number;
                    ViewBag.StaffEmail = staff.Email;
                }



            }
            else
            {
                /* *
                 * Get the message for successful booking or editing of appointment
                 * */


                int staffId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StaffId);
                Branch_Staff br_st = db.Branch_Staff.SingleOrDefault(x => x.Staff_Id == staffId);

                /* *
                 * Gets the cases assigned to the staff and branch details of the staff
                 * */
                Branch br = db.Branches.SingleOrDefault(x => x.Branch_Id == br_st.Branch_Id);
                var cases = from cs in db.Case_Staff
                            from c in db.Cases
                            where cs.Case_Id == c.Case_Id && cs.Staff_Id == staffId
                            select c;

                /* *
                 * Gets the appointments where the client has requested for an appointment and technically where the 
                 * case id is null which is during Registration phase
                 * */
                DateTime current = DateTime.Now;
                var appoints = db.Appointments.ToList().Where(x => (x.Case_Id == null) && (x.Confirmation == APP_REQUEST_APPROVAL) && x.Branch_Id == br.Branch_Id);
                model.clientAppointments = appoints.ToList();

                /* *
                 * Gets the appointments where the client with a case id has requested for an appointment
                 * */
                var myStudentApp = db.Appointments.ToList().Where(x => (x.Staff_Id == staffId) && (x.Confirmation.Equals(APP_REQUEST_APPROVAL))).OrderBy(x=>x.AppDateTime);
                model.myStudentAppointment = myStudentApp.ToList();

                /* *
                 * Gets the confirmed appointments regardless of case id 
                 * */
                var confirmAppWoCase = from app in db.Appointments
                                       where app.Confirmation == APP_CONFIRMED && app.Staff_Id == staffId && app.AppDateTime >= current
                                       orderby app.AppDateTime ascending
                                       select app;
                model.confirmedAppointments = confirmAppWoCase.ToList();

                /* *
                 * Gets the previous appointments completed by the counsellor
                 * */
                var previousAppointments = from app in db.Appointments
                                           where app.AppDateTime < current && app.Staff_Id == staffId
                                           orderby app.AppDateTime ascending
                                           select app;
                model.staffPreviousApp = previousAppointments.ToList();

                var caseStudents = from app in db.Appointments
                                   where app.Confirmation == APP_REQUEST_APPROVAL && app.Branch_Id == br.Branch_Id && app.Case_Id != null && app.Staff_Id == null
                                   orderby app.AppDateTime ascending
                                   select app;
                model.caseStudentAppointments = caseStudents.ToList();
            }
            return View(model);
        }
        #endregion

        #region ChangeStatus to Attended or Not Attended
        [Authorize(Roles = "Counsellor")]
        public ActionResult ChangeStatus(int id, string status)
        {
            Appointment appointment = db.Appointments.Single(a => a.Appointment_Id == id);
            if (ModelState.IsValid)
            {
                if (status == "attended")
                {
                    appointment.Confirmation = APP_ATTENDED;
                }
                else
                {
                    appointment.Confirmation = APP_NOT_ATTENDED;
                }
                db.SaveChanges();
                if (appointment.Case_Id == null)
                {
                               
                    LogHelper.writeToStudentLog((int)(appointment.Case_Id), appointment.Staff.FirstName + " Changed status of " + appointment.General_Enquiry.SingleOrDefault().Client.GivenName+" to "+ appointment.Confirmation, LogHelper.LOG_UPDATE, LogHelper.SECTION_APPOINTMENT);
                }
                else
                {
                    LogHelper.writeToStudentLog((int)(appointment.Case_Id), appointment.Staff.FirstName + " Changed status of " + appointment.Case.Student.Client.GivenName + " to " + appointment.Confirmation, LogHelper.LOG_UPDATE, LogHelper.SECTION_APPOINTMENT);
                }
                return RedirectToAction("Index");

            }
            return View();
        }
        #endregion

        #region Details 
        [Authorize(Roles = "Student")]
        public ViewResult Details(int id)
        {
            Appointment appointment = db.Appointments.Single(a => a.Appointment_Id == id);
            return View(appointment);
            
        }
        #endregion
        
        #region Book Appointment(Create)
        [Authorize(Roles ="Student,Counsellor")]
        public ActionResult Create()
        {
            if (TempData[NO_BOOK] != null)
            {
                ViewBag.NoBook = true;
            }
            if (TempData[SUCCESS_BOOK] != null)
            {
                ViewBag.SuccessBook = true; 
            }
            
            ViewBag.Hours = new SelectList(TimeHelper.GetHours(), "Hours", "Hours");
            ViewBag.Minutes = new SelectList(TimeHelper.GetMinutes(), "Minutes", "Minutes");

            if (StormWeb.Helper.CookieHelper.isStaff())
            {
               

                /* *
                 * Gets the specific student via check appointment for whom the counsellor wants to book an appointment with
                 * */
                    if (Request.QueryString["studentId"] != null)
                    {
                        int studentId = Convert.ToInt32(Request.QueryString["studentId"]);

                        Client client = (from st in db.Students
                                         from cl in db.Clients
                                         where st.Student_Id == studentId && st.Client_Id == cl.Client_Id
                                         select cl).Single();

                        ViewBag.specificStudent = client.GivenName;
                    }
                    else
                    {
                        /* *
                         * Retriving all students specific to a particular counselor
                         * */

                        int staffId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StaffId);
                        var student = from caseStaff in db.Case_Staff
                                      from cases in db.Cases
                                      from stud in db.Students
                                      where caseStaff.Case_Id == cases.Case_Id && cases.Student_Id == stud.Student_Id && caseStaff.Staff_Id == staffId
                                      select stud.Client.GivenName;
                        SelectList studs = new SelectList(student);
                        ViewBag.staffSpecificStudent = studs.ToList();
               
                    }
            }
            else
            {
                int studentId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StudentId);
                /* *
                 * Retrieve branch of the corresponding staff during STUDENT login
                 * */
                Branch br = (from cas in db.Cases
                             from bran in db.Branches
                             where cas.Branch_Id == bran.Branch_Id && cas.Student_Id == studentId
                             select bran).Single();
                ViewBag.BranchName = br.Branch_Name;
            }
            return View();
        }


        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = "Student,Counsellor")]
        public ActionResult Create(Appointment appointment, FormCollection fc)
        {
            #region Populating Drop down list for Hours,Minutes and Student
            string studentName = fc["staffSpecificStudent"];
            
            if (studentName != "" || studentName != null)
            {
                ViewBag.name = studentName;
            }
            
            if (fc["listHours"] == "" && fc["listMinutes"] == "")
            {
                ViewBag.Hours = new SelectList(TimeHelper.GetHours(), "Hours","Hours");
                ViewBag.Minutes = new SelectList(TimeHelper.GetMinutes(), "Minutes","Minutes");
            }
            else
            {
                ViewBag.Hours = new SelectList(TimeHelper.GetHours(), "Hours", "Hours", fc["listHours"]);
                ViewBag.Minutes = new SelectList(TimeHelper.GetMinutes(), "Minutes", "Minutes", fc["listMinutes"]);
            }
            if (StormWeb.Helper.CookieHelper.isStaff())
            {
                int staffId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StaffId);

                /* *
                 * Retrieving all students specific to a particular counselor
                 * */

                Branch_Staff br = db.Branch_Staff.Single(x => x.Staff_Id == staffId);
                var student = from caseStaff in db.Case_Staff
                              from cases in db.Cases
                              from stud in db.Students
                              where caseStaff.Case_Id == cases.Case_Id && cases.Student_Id == stud.Student_Id && caseStaff.Staff_Id == staffId
                              select stud.Client.GivenName;


                SelectList studs = new SelectList(student, fc["staffSpecificStudent"]);

                ViewBag.staffSpecificStudent = studs.ToList();
                studentName = fc["staffSpecificStudent"];

            }
            else
            {
                int stuId = Convert.ToInt32(CookieHelper.StudentId);
                Client cl = (from st in db.Students
                            from cli in db.Clients
                            where st.Student_Id == stuId && st.Client_Id == cli.Client_Id
                            select cli).Single();
                studentName = cl.GivenName; 
            }
            #endregion

            

            if (studentName == "" || studentName == null)
            {
                ModelState.AddModelError("StudentEmpty", "Please select a student!");
                return View(appointment);
            }

            DateTime dt = DateTime.MinValue;
            DateTime dateSelected = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            string comments = fc["Comments"];

            /* *
             * Getting the time from the create view
             * */

            string listHours = fc["listHours"];
            string listMinutes = fc["listMinutes"];
            if ((listHours == "" && listMinutes == "") || listHours == "" || listMinutes == "")
            {
                ModelState.AddModelError("HourMinuteError", "Select Time");
                return View(appointment);
            }

            /* *
             * Getting the date from the create view
             * */
            string dateBase = fc["AppDateTime"];
            try
            {
                dt = DateTime.ParseExact(dateBase, "d", null);
            }
            catch (FormatException)
            {
                ModelState.AddModelError("DateSelection", "Try selecting another date");
                return View(appointment);
            }
            dateSelected = new DateTime(dt.Year, dt.Month, dt.Day, Convert.ToInt32(listHours), Convert.ToInt32(listMinutes), 00);

            /* *
             * Checking if the date selected is above the current date
             * */
            if (dateSelected < currentDate)
            {

                ModelState.AddModelError("AppDateTime", "Please select a date above the current date");
                return View(appointment);
            }
            else if (dateBase == "")
            {
                ModelState.AddModelError("DateEmpty", "The date field cannot be left blank");
                return View(appointment);
            }
            if (StormWeb.Helper.CookieHelper.isStaff())
            {
                int staffId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StaffId);
                Branch_Staff br = db.Branch_Staff.Single(x => x.Staff_Id == staffId);
                /* *
                * Gets the specific student via check appointment for whom the counsellor wants to book an appointment with
                * */
                /*if (Request.QueryString["studentId"] != null)
                {
                    int studentId = Convert.ToInt32(Request.QueryString["studentId"]);

                    Client client = (from st in db.Students
                                     from cl in db.Clients
                                     where st.Student_Id == studentId && st.Client_Id == cl.Client_Id
                                     select cl).Single();

                    ViewBag.specificStudent = client.GivenName;
                }*/

                if (ViewBag.specificStudent != null)
                {
                    studentName = (string)ViewBag.specificStudent;
                }
                

                /* *
                 * Getting the student selected by the counselor and retrieving the caseId specific to the 
                 * student
                 * */
                
                Case c = (from s in db.Students
                          from cl in db.Clients
                          from cas in db.Cases
                          where cl.GivenName == studentName && cl.Client_Id == s.Client_Id && cas.Student_Id == s.Student_Id
                          select cas).Single();
                
                var appWithStudent = from app in db.Appointments
                                   where app.Staff_Id == staffId && app.Case_Id == c.Case_Id && ((app.Confirmation == APP_CONFIRMED) || (app.Confirmation == APP_REQUEST_APPROVAL && app.AppDateTime > DateTime.Now))
                                   select app;

                if (appWithStudent.Count() > 0)
                {
                    TempData[NO_BOOK] = true;
                    NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "You can only book a single appointment with a student!");
                    //return RedirectToAction("Create", new { m  = "No Booking"});
                    return View("Refresh");
                }

                appointment.Confirmation = APP_CONFIRMED;
                appointment.AppDateTime = dateSelected;
                appointment.Case_Id = c.Case_Id;
                appointment.Branch_Id = br.Branch_Id;
                appointment.Comments = comments;
                appointment.Staff_Id = staffId;


                if (ModelState.IsValid)
                {
                    db.Appointments.AddObject(appointment);
                    db.SaveChanges();
                    TempData[SUCCESS_BOOK] = true;
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Your Appointment was successfully booked");
                    //return RedirectToAction("Create", new { message = "Successfully Booked" });
                    LogHelper.writeToStudentLog(Convert.ToInt32(appointment.Case_Id), appointment.Staff.FirstName +" booked an appointment for "+appointment.Case.Student.Client.GivenName, LogHelper.LOG_CREATE, LogHelper.SECTION_APPOINTMENT);
                    //return RedirectToAction("Index");
                    return View("Refresh");
                }
            }
            else
            {
                int studentId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StudentId);
                Case c = db.Cases.Single(x => x.Student_Id == studentId);
                Case_Staff s = db.Case_Staff.SingleOrDefault(x => x.Case_Id == c.Case_Id && x.Role == "Counsellor");
                appointment.Confirmation = APP_REQUEST_APPROVAL;
                appointment.AppDateTime = dateSelected;
                appointment.Case_Id = c.Case_Id;
                appointment.Branch_Id = c.Branch_Id;
                appointment.Comments = comments;
                if (s != null)
                {
                    appointment.Staff_Id = s.Staff_Id;
                }
                if (ModelState.IsValid)
                {
                    db.Appointments.AddObject(appointment);
                    db.SaveChanges();
                    TempData[SUCCESS_BOOK] = true;
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Your Appointment was successfully booked");
                    //return RedirectToAction("Create", new { message = "Booked Successfully" });
                    LogHelper.writeToStudentLog(Convert.ToInt32(appointment.Case_Id), "Appointment booked by "+appointment.Case.Student.Client.GivenName, LogHelper.LOG_CREATE, LogHelper.SECTION_APPOINTMENT);
                    //return RedirectToAction("Index");
                    return View("Refresh", new RefreshModel(Url.Action("Index")) );
                }
            }
            ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy", appointment.Case_Id);

            return View(appointment);
        }
        #endregion
       
        #region Edit/Rescedule/Confirm Appointment
        [Authorize(Roles = "Student,Counsellor")]
        public ActionResult Edit(int id)
        {
            if (TempData[SUCCESS_EDIT] != null)
            {
                ViewBag.SuccessEdit = true;
            }
            Appointment appointment = db.Appointments.Single(a => a.Appointment_Id == id);
            ViewBag.Hours = new SelectList(TimeHelper.GetHours(), "Hours", "Hours",appointment.AppDateTime.Hour);
            ViewBag.Minutes = new SelectList(TimeHelper.GetMinutes(), "Minutes", "Minutes",appointment.AppDateTime.Minute);
            //ViewBag.Case_Id = new SelectList(db.Cases, "Case_Id", "CreatedBy", appointment.Case_Id);
            return View(appointment);
        }

        //
        // POST: /Appointment/Edit/5

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = "Student,Counsellor")]
        public ActionResult Edit(Appointment appointment, FormCollection fc)
        {
            int appId = Convert.ToInt16(fc["Appointment_Id"]);
            appointment = db.Appointments.Single(x => x.Appointment_Id == appId);
            if (fc["listHours"] == "" && fc["listMinutes"] == "")
            {
                ViewBag.Hours = new SelectList(TimeHelper.GetHours(), "Hours", "Hours");
                ViewBag.Minutes = new SelectList(TimeHelper.GetMinutes(), "Minutes", "Minutes");
            }
            else
            {
                ViewBag.Hours = new SelectList(TimeHelper.GetHours(), "Hours", "Hours", fc["listHours"]);
                ViewBag.Minutes = new SelectList(TimeHelper.GetMinutes(), "Minutes", "Minutes", fc["listMinutes"]);
            }

            int staffId = 0;
            DateTime dateTemp = DateTime.MinValue;
            int listHours = Convert.ToInt32(fc["listHours"]);
            int listMinutes = Convert.ToInt32(fc["listMinutes"]);
            string timeFromForm = listHours + ":" + listMinutes + ":" + "00";

            string s= fc["Appointment_Id"]; 

            if (fc["AppDateTime" + s] == null)
            {
                ModelState.AddModelError("DateError", "Date cannot be null");
                return View(appointment);
            }
            string datebase = fc["AppDateTime" + s];

            DateTime currentDate = DateTime.Now;
            try
            {
                dateTemp = DateTime.ParseExact(datebase, "d", null);
            }
            catch (FormatException)
            {

            }
            DateTime dateSelected = new DateTime(dateTemp.Year, dateTemp.Month, dateTemp.Day, listHours, listMinutes, 00);
            
            /* *
             * Checking if the appointment date has been booked out
             * */
            if (StormWeb.Helper.CookieHelper.isStaff())
            {
                staffId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StaffId);
                Appointment appnmnt = (from app in db.Appointments
                                where app.AppDateTime == dateSelected && app.Confirmation == APP_CONFIRMED && app.Staff_Id == staffId
                                select app).FirstOrDefault();

                if (appnmnt != null)
                {
                    ModelState.AddModelError("AppBookedOut", "This appointment time has been taken. Please choose another time or date");
                    return View(appointment);
                }

            }
            if (dateSelected < currentDate)
            {
                ModelState.AddModelError("AppDateTime", "Please select a date above the current date");
                return View(appointment);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    if (StormWeb.Helper.CookieHelper.isStaff())
                    {
                        staffId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StaffId);
                    }
                    appointment.AppDateTime = dateSelected; ;
                    appointment.Confirmation = APP_CONFIRMED;
                    appointment.Staff_Id = staffId;
                    if (fc["Case_Id"] != null)
                    {
                        appointment.Case_Id = Convert.ToInt32(fc["Case_Id"]);
                    }
                    appointment.Comments = fc["Comments"];
                    //db.Appointments.Attach(appointment);

                    db.ObjectStateManager.ChangeObjectState(appointment, EntityState.Modified);
                    db.SaveChanges();
                    TempData[SUCCESS_EDIT] = true;
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Appointment has been rescheduled/Confirmed");
                    //return RedirectToAction("Edit", new { mess = "EditSuccess" });
                    if (appointment.Case_Id != null)
                    {
                        LogHelper.writeToStudentLog(new string[] { CookieHelper.Username },CookieHelper.Username +" Edited " + appointment.Case.Student.Client.GivenName + "'s appointment", LogHelper.LOG_OTHER, LogHelper.SECTION_APPOINTMENT);
                    }
                    //return RedirectToAction("Index");
                    return View("Refresh");

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} First exception caught.", e);
                Debug.WriteLine(e.InnerException);
              //  LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, "Error:" + e.InnerException.ToString(), LogHelper.LOG_OTHER, LogHelper.SECTION_APPOINTMENT);
                ModelState.AddModelError("", e);
            }
            return View(appointment);
        }
        #endregion

        #region Delete 
       
        public ActionResult Delete(int id)
        {
            Appointment appointment = db.Appointments.Single(a => a.Appointment_Id == id);
            
            if (appointment.Case_Id == null)
            {
                General_Enquiry genEnquiry = db.General_Enquiry.Single(g => g.Appointment_Id == id);
                try
                {
                    LogHelper.writeToStudentLog((int)(appointment.Case_Id),appointment.Staff.FirstName + " Deleted " + genEnquiry.Client.GivenName + "'s General Enquiry appointment entry", LogHelper.LOG_DELETE, LogHelper.SECTION_APPOINTMENT);
                    db.General_Enquiry.DeleteObject(genEnquiry);
                    db.SaveChanges();
                    
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            LogHelper.writeToStudentLog((int)(appointment.Case_Id), appointment.Staff.FirstName + " Deleted " + appointment.Case.Student.Client.GivenName + "'s appointment", LogHelper.LOG_OTHER, LogHelper.SECTION_APPOINTMENT);
            db.Appointments.DeleteObject(appointment);
            db.SaveChanges();
            NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Appointment Deleted");
            return RedirectToAction("Index");
        }
        #endregion

        #region Checking for an appointment
        [Authorize(Roles = "Student,Counsellor")]
        public ViewResult CheckAppointment(int id)
        {
            Appointment appointment = null;
            if (StormWeb.Helper.CookieHelper.isStaff())
            {
                ViewBag.studentId = id;
                int staffId = Convert.ToInt32(StormWeb.Helper.CookieHelper.StaffId);
                Case cases = db.Cases.Single(x => x.Student_Id == id);
                DateTime current = DateTime.Now; 
                var a = (from appo in db.Appointments
                               where appo.Case_Id == cases.Case_Id && (appo.Confirmation == APP_CONFIRMED || appo.Confirmation == APP_REQUEST_APPROVAL) && appo.AppDateTime >= current && appo.Staff_Id == staffId
                               orderby appo.AppDateTime ascending
                               select appo);
                if(a.Count()>0)
                appointment = a.First();
            }
            return View(appointment);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #region CODE
        public static string SUCCESS_BOOK = "SuccessfulBook";
        public static string SUCCESS_EDIT = "SuccessfulEdit";
        public static string NO_BOOK = "NoBook";
        #endregion
    }
}