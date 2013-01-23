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
    public class LogController : Controller
    {
        private StormDBEntities db = new StormDBEntities();

        //
        // GET: /Log/
        //This is for only Log students/
        [Authorize(Roles="Student,Super")]
        public ViewResult Index() //This is for Log/Index
        {
            int studentId=0;
            string username = "";
            if (CookieHelper.isStudent())
            {
                studentId = Convert.ToInt32(CookieHelper.StudentId);
                username = CookieHelper.Username;
            }
            //var logs = db.Logs.Include("Case");
             
            //Case studentCase = db.Cases.Single(x => x.Student_Id == studentId);
            return View(db.Student_Log.Where(x => x.UserName == username).ToList().OrderByDescending(x=>x.DateTime));
        }


        [Authorize(Roles = "Counsellor,Super,BranchManager")]
        public ViewResult ShowLogs()
        {
            int staffId = 0;
            staffId = Convert.ToInt32(CookieHelper.StaffId);

            var cases = from c in db.Cases
                        from cs in db.Case_Staff
                        where c.Case_Id == cs.Case_Id && cs.Staff_Id == staffId
                        select c;

            //var cases = db.Cases.ToList().Where(x => x.Counsellor_Id == staffId);

            // To be added logs for super and branch manager
            if (CookieHelper.isInRole("Super") || CookieHelper.isInRole("BranchManager"))
            {

            }

            List<Client> clients = new List<Client>();

            foreach (Case c in cases)
            {
                Student newStudent = db.Students.Single(x => x.Student_Id == c.Student_Id);
                Client newClient = db.Clients.Single(x => x.Client_Id == newStudent.Client_Id);
                clients.Add(newClient);
            }

            return View(clients);
        }

        // localhost/Log/Student/StudentId
        [Authorize(Roles = "Counsellor")]
        public ActionResult Student(int id)
        {
            int staffId = 0;
            if (CookieHelper.isStaff())
            {
                staffId = Convert.ToInt32(CookieHelper.StaffId);
                if (!StudentsHelper.staffAssignedToStudent(staffId, id))
                {
                    return RedirectToAction("BadLink", "Errors", new { message = "Student is not assigned to you" });
                }

                string username = db.Students.SingleOrDefault(x => x.Student_Id == id).UserName;

                var theLogs = db.Student_Log.Where(x => x.UserName == username).ToList();

                return View(theLogs);

            }
            NotificationHandler.setNotification(NotificationHandler.NOTY_ERROR, "Bad links!");
            return RedirectToAction("Home", "Index");
        }

        [HttpPost]
        public ContentResult getLogs(int id)
        {
            string username = CookieHelper.Username;


            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            string jsonResult = serializer.Serialize(db.Student_Log.Where(x => x.UserName == username).OrderByDescending(x => x.DateTime).Skip(id * 5).Take(5).ToList());

            return new ContentResult{ Content = jsonResult, ContentType = "application/json"};
        }

        
    }
}