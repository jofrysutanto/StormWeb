// --------------------------------------------------------------------------------------------------------------------
// <summary>
// File Name    : CourseController.cs
// Created Date : 14/08/2011
// Created By   : Manali Modi
// Description  : All information related to the courses.
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
using System.Data.Objects.SqlClient;
using System.Diagnostics;

using StormWeb.Helper;

namespace StormWeb.Controllers
{
    public class CourseController : Controller
    {
        private StormDBEntities db = new StormDBEntities();
         
        [Authorize(Roles = "Super")]
        public ViewResult List()
        {
            var courses = db.Courses.Include("Course_Level").Include("Faculty");
            return View(courses.ToList());
        }

        [Authorize(Roles = "Super")]
        public ViewResult ViewCourses(int id)
        {
            //var courses = db.Courses.Include("Course_Level").Include("Faculty");
            var fac = db.Faculties.ToList().Where(x => x.University_Id == id);
            foreach (var item in fac)
            {
                var cou = db.Courses.ToList().Where(x => x.Faculty_Id == item.Faculty_Id);
                return View(cou);
            }
            return View();
        }

        [Authorize(Roles = "Student,Counsellor")]
        public ActionResult Index(int id = -1)
        {
            int studentId = -1;
            if (id == -1)
            {
                studentId = CookieHelper.getStudentId();
            }
            else
            {
                studentId = id;
                ViewBag.studentIdC = id;
                if (CookieHelper.isStaff())
                {
                    if (!StudentsHelper.staffAssignedToStudent(CookieHelper.getStaffId(), studentId))
                    {
                        return RedirectToAction("BadLink", "Errors", new { message="Student is not assigned to you" });
                    }
                }
            }

            if (!ValidationHelper.isStudent(studentId))
            {
                return RedirectToAction("BadLink", "Errors");
            }

            var courses = db.Courses.Include("Course_Level").Include("Faculty");


            var studentApplication = from c in db.Courses
                                     from app in db.Applications
                                     where app.Student_Id == studentId && app.Course_Id == c.Course_Id
                                     select c;

            ViewBag.CountApplication = studentApplication.Count();

            return View(courses.Except(studentApplication).ToList());
        }
 
        [Authorize(Roles = "Super")]
        public ViewResult Details(int id)
        {
            Course course = db.Courses.Single(c => c.Course_Id == id);
            return View(course);
        }
         
        [Authorize(Roles = "Super")]

        #region CREATE

        public ActionResult Create()
        {
            ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1");
            ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name");

            ViewBag.Universities = from u in db.Universities
                                   select new SelectListItem
                                   {
                                       Text = u.University_Name,
                                       Value = SqlFunctions.StringConvert((double)u.University_Id),
                                   };

            return View();

        }
         
        [Authorize(Roles = "Super")]

        [HttpPost]
        public ActionResult Create(Course course, FormCollection fc)
        {

            ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1");
            ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name");

            ViewBag.Universities = from u in db.Universities
                                   select new SelectListItem
                                   {
                                       Text = u.University_Name,
                                       Value = SqlFunctions.StringConvert((double)u.University_Id),
                                   };           
            if (ModelState.IsValid)
            {
                if (course.Commence_Date_Sem < DateTime.Now)
                {
                    ModelState.AddModelError("DateError", "Please enter valid date");

                }
                else
                {
                    db.Courses.AddObject(course);
                    db.SaveChanges();
                    return RedirectToAction("List");
                }
            }
              
                
            if (fc["Course_Level_Id"] == "")
            {
                ModelState.AddModelError("CourseLevelError", "Please select Course Level");
            }
            
            if (fc["Course_Name"] == "")
            {
                ModelState.AddModelError("CourseNameError", "Please enter Course Name");
            }
            if (fc["Duration"] == "")
            {
                ModelState.AddModelError("DurationError", "Please enter Duration");
            }
            if (fc["Fee"] == "")
            {
                ModelState.AddModelError("FeeError", "Please enter Fee");
            }
            else
            {                
                course.Course_Level_Id = Convert.ToInt32(fc["Course_Level_Id"]);                
                course.Course_Name = Convert.ToString(fc["Course_Name"]);
                course.Fee = Convert.ToInt32(fc["Fee"]);
            }
          
            ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
            ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added a new Course "+ course.Course_Name), LogHelper.LOG_CREATE, LogHelper.SECTION_COURSE);
            return View(course);
        }

        #endregion

        [Authorize(Roles = "Super")]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetFaculties(int uniID = -1)
        {
            if (uniID == -1)
                return Json(Enumerable.Empty<SelectListItem>());           

            IEnumerable<SelectListItem> selectList = from f in db.Faculties
                                                     where f.University_Id == uniID
                                                     select new SelectListItem
                                                     {
                                                         Text = f.Faculty_Name,                                                         
                                                         Value = SqlFunctions.StringConvert((double)f.Faculty_Id)
                                                     };

            return Json(selectList);
        }

        #region EDIT

        [Authorize(Roles = "Super")]
        public ActionResult Edit(int id)
        {
            Course course = db.Courses.Single(c => c.Course_Id == id);
            ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
            ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
            return View(course);
        }
         
        [Authorize(Roles = "Super")]
        [HttpPost]
        public ActionResult Edit(Course course,FormCollection fc)
        {
           
            if (ModelState.IsValid)
            {
                if (course.Commence_Date_Sem < DateTime.Now)
                {
                    ModelState.AddModelError("DateError", "Please enter valid date");
                }
                else
                {
                    db.Courses.Attach(course);
                    db.ObjectStateManager.ChangeObjectState(course, EntityState.Modified);
                    db.SaveChanges();
                    return RedirectToAction("List");
                }
            }
            if (fc["Faculty_Select"] == "")
            {
                ModelState.AddModelError("FacultyError", "Please select a faculty");
            }
            if (fc["Course_Level_Id"] == "")
            {
                ModelState.AddModelError("CourseLevelError", "Please select Course Level");
            }            
            if (fc["Course_Name"] == "")
            {
                ModelState.AddModelError("CourseNameError", "Please enter Course Name");
            }
            if (fc["Duration"] == "")
            {
                ModelState.AddModelError("DurationError", "Please enter Duration");
            }
            if (fc["Fee"] == "")
            {
                ModelState.AddModelError("FeeError", "Please enter Fee");
            }
            else
            {
                course.Faculty_Id = Convert.ToInt32(fc["Faculty_Select"]);
                course.Course_Level_Id = Convert.ToInt32(fc["Course_Level_Id"]);               
                course.Course_Name = Convert.ToString(fc["Course_Name"]);
                course.Duration = Convert.ToInt32(fc["Duration"]);
                course.Fee = Convert.ToInt32(fc["Fee"]);
            }
            
            ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
            ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited the new Course " + course.Course_Name), LogHelper.LOG_UPDATE, LogHelper.SECTION_COURSE);
            return View(course);
        }

        #endregion

        #region DELETE

        [Authorize(Roles = "Super")]
        public ActionResult Delete(int id)
        {
            Course course = db.Courses.Single(c => c.Course_Id == id);
            return View(course);
        }
         
        [Authorize(Roles = "Super")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Course course = db.Courses.Single(c => c.Course_Id == id);
             try
            {
                db.Courses.DeleteObject(course);
                db.SaveChanges();
                LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Deleted the Course " + course.Course_Name), LogHelper.LOG_DELETE, LogHelper.SECTION_COURSE);
                return RedirectToAction("List");
            }
             catch (Exception e)
             {
                 Debug.WriteLine(e.Message);

                 ModelState.AddModelError(String.Empty, "Cannot delete");
                 return View(course);
             }    
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}