using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;

namespace StormWeb.Controllers
{
    public class StudentStaffController : Controller
    {
        private static StormDBEntities db = new StormDBEntities();

        #region List of Students assigned to a staff

        public ActionResult Index()
        {

            return View(getStudentStaffModel());
        }

        public static StudentStaffModel getStudentStaffModel()
        {
            StormDBEntities db = new StormDBEntities();
            StudentStaffModel ssModel = new StudentStaffModel();
            int staffId = 0;
            if (CookieHelper.isStaff())
            {
                staffId = Convert.ToInt32(CookieHelper.StaffId);
            }

            // returning staff id and case id
            var caseStaff = db.Case_Staff.ToList().Where(c => c.Staff_Id == staffId);
            ssModel.case_StaffTable = caseStaff.ToList();

            ssModel.caseTable = new List<Case>();
            foreach (var item in caseStaff)
            {
                ssModel.caseTable.Add(db.Cases.Include("Applications").Single((c => c.Case_Id == item.Case_Id)));
            }
            return ssModel;
        }


        #endregion

        /// <summary>
        /// List all students with no staff assigned
        /// Only shows the students that the staff can be assigned to (from the same branch)
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Counsellor")]
        public ActionResult NewStudent()
        {
            // Get the branch currently assigned to counsellor
            var branchList = BranchHelper.getBranchListFromCookie();

            var studentWithNoStaff = from b in branchList
                                     from c in db.Cases
                                     where !(from cs in db.Case_Staff
                                             select cs.Case_Id).Contains(c.Case_Id) && b.Branch_Id == c.Branch_Id
                                     select c;

            if (TempData["message"] != null)
            {
                ViewBag.Message = "success";
            }

            return View(studentWithNoStaff.ToList());
        }

        [Authorize(Roles = "Counsellor")]
        public static int countNewStudent()
        {
            // Get the branch currently assigned to counsellor
            var branchList = BranchHelper.getBranchListFromCookie();

            var studentWithNoStaff = from b in branchList
                                     from c in db.Cases
                                     where !(from cs in db.Case_Staff
                                             select cs.Case_Id).Contains(c.Case_Id) && b.Branch_Id == c.Branch_Id
                                     select c;

            return studentWithNoStaff.Count();
        }


        [Authorize(Roles = "Counsellor")]
        [HttpGet]
        public ActionResult AssignStudent(int id)
        {
            Case_Staff cs = new Case_Staff();
            cs.Case_Id = id;
            cs.Staff_Id = CookieHelper.getStaffId();
            cs.Role = "Counsellor";

            db.Case_Staff.AddObject(cs);
            db.SaveChanges();

            TempData["message"] = "success";

            LogHelper.writeToStudentLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Assigned a new student " + cs.Case.Student.Client.GivenName + " " + cs.Case.Student.Client.LastName + " to the Staff " + cs.Staff.FirstName + " " + cs.Staff.LastName), LogHelper.LOG_CREATE, LogHelper.SECTION_PROFILE);

            return RedirectToAction("NewStudent");
        }

        //
        // GET: /StudentStaff/Details/5
        [Authorize(Roles = "Counsellor")]
        public ActionResult Details(int id)
        {

            StudentStaffModel ssModel = new StudentStaffModel();
            var app = db.Applications.ToList().Where(a => a.Case_Id == id);
            ssModel.applicationTable = app.ToList();
            return View(ssModel);
        }
        [Authorize(Roles = "Counsellor")]
        public ActionResult Documents(int id)
        {
            StudentStaffModel ssModel = new StudentStaffModel();
            var doc = db.Application_Document.ToList().Where(d => d.Application_Id == id);
            ssModel.applicationDocumentTable = doc.ToList();
            return View(ssModel);
        }


        //
        // GET: /StudentStaff/Create
        [Authorize(Roles = "Counsellor")]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /StudentStaff/Create

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
        // GET: /StudentStaff/Edit/5
        [Authorize(Roles = "Counsellor")]
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /StudentStaff/Edit/5
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
       

        //Currently working

        [Authorize(Roles = "Counsellor")]
        public ActionResult SwitchStaff(int id)
        {
            try
            {
                if (StormWeb.Helper.CookieHelper.isStaff())
                {
                    /* *
                     * Retriving all Cases specific to a particular counselor
                     * */
                    ViewBag.studentId = id;
                    int staffId = Convert.ToInt32(CookieHelper.StaffId);
                    int branchId = db.Students.SingleOrDefault( x=> x.Student_Id == id).Client.Branch_Id;

                    var staffName = from branchStaff in db.Branch_Staff
                                    from s in db.Staffs
                                    where s.Staff_Id == branchStaff.Staff_Id && branchStaff.Branch_Id == branchId
                                    select (s.FirstName + " " + s.LastName);
                                    
                  
                    //var cases = (from casestaff in db.Case_Staff
                    //             join sts in db.Staffs on casestaff.Staff_Id equals sts.Staff_Id
                    //             where casestaff.Staff_Id == sts.Staff_Id && casestaff.Role == "Counsellor"
                    //             select sts.FirstName).Distinct();
                    SelectList staff = new SelectList(staffName);
                    ViewBag.staffWithCases = staff.ToList();

                    //Update case staff table where staff id= new staff id
                    StudentStaffModel ssModel = new StudentStaffModel(); 
                }
                /*    var doc = db.Application_Document.ToList().Where(d => d.Application_Id == id);
                    ssModel.applicationDocumentTable = doc.ToList();*/
                return View();
            }
            catch
            {
                return View();

            }
        }

        [HttpPost]
        [Authorize(Roles = "Counsellor")]
        public ActionResult SwitchStaff(FormCollection frms,int id)
        {
            string staffcase = frms["staffWithCases"];
             
            int student = Convert.ToInt32(frms["studentId"]);

            if (staffcase == "")
            {
                ModelState.AddModelError("StaffEmpty", "Please select a Staff!");
                //  return View(appointment);
            } 
            if (StormWeb.Helper.CookieHelper.isStaff())
            {
                /* *
                 * Retriving all Cases specific to a particular counselor
                 * */
                int staffId = Convert.ToInt32(CookieHelper.StaffId);

                // Get the branch id of student
                int branchId = db.Students.SingleOrDefault(x => x.Student_Id == id).Client.Branch_Id;

                // List of staff names
                var staffName = from branchStaff in db.Branch_Staff
                                from s in db.Staffs
                                where s.Staff_Id == branchStaff.Staff_Id && branchStaff.Branch_Id == branchId
                                select (s.FirstName + " " + s.LastName);

                // Retrieve first and last name of the new staff
                string fname = staffcase.Split(' ')[0];
                string lname = staffcase.Split(' ')[1];




                ////Retrieve first name and last name of the old staff
                //string fstname = staffcase.Split(' ')[0];
                //string lstname = staffcase.Split(' ')[1];
                //Staff oldStaff = (from s in db.Staffs
                //                  where s.FirstName == fstname && s.LastName == lstname
                //                  select s).SingleOrDefault();




                Student st = db.Students.SingleOrDefault(x => x.Student_Id == student);
                // Return new staff object
                Staff newStaff = (from s in db.Staffs
                                 where s.FirstName == fname && s.LastName == lname
                                 select s).SingleOrDefault();
                // Get the case of the student
                int caseId = db.Students.SingleOrDefault(x => x.Student_Id == student).Cases.First().Case_Id;
                //Checking if the counsellor has any appointments with the student before assigning the student to another staff
                Appointment appointment = db.Appointments.SingleOrDefault(x => x.Staff_Id == staffId && (x.Confirmation== "Request for Approval" || x.Confirmation=="Confirmed") && x.AppDateTime > DateTime.Now && x.Case_Id == caseId);
                if (appointment != null)
                {
                    //LogHelper.writeToLog(caseId, st.Client.GivenName + "'s existing appointment was cancelled because his counsellor" + fstname + "is changed to " + fname, LogHelper.LOG_DELETE, LogHelper.SECTION_APPOINTMENT);
                    LogHelper.writeToStudentLog(caseId, st.Client.GivenName + "'s existing appointment was cancelled because his counsellor is changed to " + fname, LogHelper.LOG_DELETE, LogHelper.SECTION_APPOINTMENT);
                    db.Appointments.DeleteObject(appointment);
                    db.SaveChanges();
                }

                // Retrieve Case_Staff object and change the staff assigned to the case
                Case_Staff cs = db.Case_Staff.SingleOrDefault(x => x.Case_Id == caseId && x.Staff_Id == staffId);
                //string oldStaff = cs.Staff.FirstName;
                cs.Staff_Id = newStaff.Staff_Id;

                // Save the changes
                db.SaveChanges();

                SelectList staff = new SelectList(staffName);
                ViewBag.staffWithCases = staff.ToList();
                //Update case staff table where staff id= new staff id
                StudentStaffModel ssModel = new StudentStaffModel();
                
                LogHelper.writeToStudentLog(caseId,  st.Client.GivenName+ "'s counsellor was changed to " + fname, LogHelper.LOG_OTHER, LogHelper.SECTION_ACCOUNT);
                NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Successfull!");
                return RedirectToAction("Index");
            }
            /*    var doc = db.Application_Document.ToList().Where(d => d.Application_Id == id);
                ssModel.applicationDocumentTable = doc.ToList();*/
            return View();
        }

        //
        // GET: /StudentStaff/Delete/5
        [Authorize(Roles = "Counsellor")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /StudentStaff/Delete/5
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
    }
}
