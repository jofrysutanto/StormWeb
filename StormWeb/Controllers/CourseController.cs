﻿// --------------------------------------------------------------------------------------------------------------------
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

        [Authorize(Roles = "Counsellor,Super,BranchManager,Administrator")]
        public ViewResult List()
        {
            var courses = db.Courses.Include("Course_Level").Include("Faculty");
            return View(courses.ToList());
        }
        /*public ViewResult List(int id)
        {
            var courses = from c in db.Courses
                          from fac in db.Faculties
                          where fac.University_Id == id && fac.Faculty_Id == c.Faculty_Id
                          select c;
            return View(courses.ToList());
        }*/

        [Authorize(Roles = "Counsellor,Super,BranchManager,Administrator")]
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


        #region Index
        [Authorize(Roles = "Student,Counsellor")]
        public ActionResult Index(int id = -1)
        {
            int studentId = -1;
            if (id == -1)
            {
                studentId = CookieHelper.getStudentId();
                ViewBag.studentIdC = studentId;
            }
            else
            {
                studentId = id;
                ViewBag.studentIdC = id;
                if (CookieHelper.isStaff())
                {
                    if (!StudentsHelper.staffAssignedToStudent(CookieHelper.getStaffId(), studentId))
                    {
                        return RedirectToAction("BadLink", "Errors", new { message = "Student is not assigned to you" });
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
        #endregion

        [Authorize(Roles = "Counsellor,Super,BranchManager,Administrator")]
        public ViewResult Details(int id)
        {
            Course course = db.Courses.Single(c => c.Course_Id == id);
            return View(course);
        }

        [Authorize(Roles = "Super,BranchManager,Administrator")]


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

        [Authorize(Roles = "Super,BranchManager,Administrator")]

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

            course.Faculty_Id = Convert.ToInt32(fc["Faculty_Select"]);
            if (ModelState.IsValid)
            {
                if (course.Commence_Date_Sem < DateTime.Now)
                {
                    ModelState.AddModelError("DateError", "Please enter valid date");
                    return View(course);
                }
                else
                {
                    db.Courses.AddObject(course);
                    db.SaveChanges();
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Course has been successfully added");
                    return RedirectToAction("List");
                }
            }


            if (fc["Course_Level_Id"] == "")
            {
                ModelState.AddModelError("CourseLevelError", "Please select Course Level");
                return View(course);
            }

            if (fc["Course_Name"] == "")
            {
                ModelState.AddModelError("CourseNameError", "Please enter Course Name");
                return View(course);
            }
            if (fc["Duration"] == "")
            {
                ModelState.AddModelError("DurationError", "Please enter Duration");
                return View(course);
            }
            if (fc["Fee"] == "")
            {
                ModelState.AddModelError("FeeError", "Please enter Fee");
                return View(course);
            }
            else
            {
                course.Course_Level_Id = Convert.ToInt32(fc["Course_Level_Id"]);
                course.Course_Name = Convert.ToString(fc["Course_Name"]);
                course.Fee = Convert.ToInt32(fc["Fee"]);
            }

            ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
            ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Added a new Course " + course.Course_Name), LogHelper.LOG_CREATE, LogHelper.SECTION_COURSE);
            return View(course);
        }

        #endregion

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetFaculties(int uniID = -1)
        {
            db.Courses.Where(x => x.Faculty.University.Country.Country_Name == "Australia").ToList();

            db.Students.Where(x => x.Student_Id == 1001).ToList();

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

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        public ActionResult Edit(int id)
        {
            Course course = db.Courses.Single(c => c.Course_Id == id);
            ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level_Name", course.Course_Level_Id);
            ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
            ViewBag.Universityddl = new SelectList(db.Universities, "University_Id", "University_Name", course.Faculty.University.University_Id);
            ViewBag.UniversityId = course.Faculty.University_Id;
            return View(course);
        }

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(Course course, FormCollection fc)
        {
            if (course.Faculty != null)
                ViewBag.UniversityId = course.Faculty.University_Id;

            int facId = Convert.ToInt32(fc["Faculty.Faculty_Id"]);
            int courseLevelId = Convert.ToInt32(fc["Course_Level.Course_Level_Id"]);

            Faculty fac = course.Faculty;
            Course_Level cou = course.Course_Level;
            course.Faculty = db.Faculties.Single(x => x.Faculty_Id == facId);
            course.Course_Level = db.Course_Level.Single(x => x.Course_Level_Id == courseLevelId);
             
            if (ModelState.IsValid)
            {
                if (course.Commence_Date_Sem < DateTime.Now)
                {
                    ModelState.AddModelError("DateError", "Please enter valid date");
                    ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level_Name", course.Course_Level_Id);
                    ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
                    ViewBag.Universityddl = new SelectList(db.Universities, "University_Id", "University_Name", course.Faculty.University.University_Id);
                    return View(course);
                }
                else
                {

                   // if (fc["Faculty.University.University_Id"] == "")
                   // {
                   //     ModelState.AddModelError("UniversityLevelError", "Please select the University");
                   //     ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
                   //     ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
                   //     ViewBag.Universityddl = new SelectList(db.Universities, "University_Id", "University_Name", course.Faculty.University.University_Id);
                   //     return View(course);
                   // }
                   // else
                   // {
                   //     //course.Faculty.University_Id = Convert.ToInt32(fc["Faculty.University.University_Id"]);
                   // }
                   // if (fc["Course_Level.Course_Level_Id"] == "")
                   // {
                   //     ModelState.AddModelError("CourseLevelError", "Please select Course Level");
                   //     ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
                   //     ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
                   //     ViewBag.Universityddl = new SelectList(db.Universities, "University_Id", "University_Name", course.Faculty.University.University_Id);
                   //     return View(course);
                   // }
                   // else
                   // {
                   //     //course.Course_Level_Id = Convert.ToInt32(fc["Course_Level.Course_Level_Id"]);
                   // }
                   // if (fc["Faculty.Faculty_Id"] == "" || fc["Faculty.Faculty_Id"] == null)
                   // {
                   //     ModelState.AddModelError("FacultyError", "Please select a faculty");
                   //     ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
                   //     ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
                   //     ViewBag.Universityddl = new SelectList(db.Universities, "University_Id", "University_Name", course.Faculty.University.University_Id);
                   //     return View(course);
                   // }
                   // else
                   // {
                   //     //course.Faculty_Id = Convert.ToInt32(fc["Faculty.Faculty_Id"]);
                   // }

                   // if (fc["Course_Name"] == "")
                   // {
                   //     ModelState.AddModelError("CourseNameError", "Please enter Course Name");
                   //     ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
                   //     ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
                   //     ViewBag.Universityddl = new SelectList(db.Universities, "University_Id", "University_Name", course.Faculty.University.University_Id);
                   //     return View(course);
                   // }
                   // if (fc["Duration"] == "")
                   // {
                   //     ModelState.AddModelError("DurationError", "Please enter Duration");
                   //     ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
                   //     ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
                   //     ViewBag.Universityddl = new SelectList(db.Universities, "University_Id", "University_Name", course.Faculty.University.University_Id);
                   //     return View(course);
                   // }
                   // if (fc["Fee"] == "")
                   // {
                   //     ModelState.AddModelError("FeeError", "Please enter Fee");
                   //     ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
                   //     ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
                   //     ViewBag.Universityddl = new SelectList(db.Universities, "University_Id", "University_Name", course.Faculty.University.University_Id);
                   //     return View(course);
                   // }
                   // else
                   // {
                   //     course.Course_Name = Convert.ToString(fc["Course_Name"]);
                   //     course.Duration = Convert.ToInt32(fc["Duration"]);
                   //     course.Fee = Convert.ToInt32(fc["Fee"]);
                   //     course.Description = fc["Description"];
                   //     course.Commence_Date_Sem = Convert.ToDateTime(fc["Commence_Date_Sem"]);
                   // }
                   //// db.Courses.Attach(course);  
                   //// db.ObjectStateManager.ChangeObjectState(course, EntityState.Modified);
                    db.ObjectStateManager.ChangeObjectState(fac, EntityState.Modified);
                    db.ObjectStateManager.ChangeObjectState(cou, EntityState.Modified);
                    db.SaveChanges();
                    NotificationHandler.setNotification(NotificationHandler.NOTY_SUCCESS, "Course has been successfully edited");
                    return RedirectToAction("ViewCourses", "Course", new { id = course.Faculty.University_Id });
                }
            }


            ViewBag.Course_Level_Id = new SelectList(db.Course_Level, "Course_Level_Id", "Course_Level1", course.Course_Level_Id);
            ViewBag.Faculty_Id = new SelectList(db.Faculties, "Faculty_Id", "Faculty_Name", course.Faculty_Id);
            ViewBag.Universityddl = new SelectList(db.Universities, "University_Id", "University_Name", course.Faculty.University.University_Id);
            LogHelper.writeToSystemLog(new string[] { CookieHelper.Username }, (CookieHelper.Username + " Edited the new Course " + course.Course_Name), LogHelper.LOG_UPDATE, LogHelper.SECTION_COURSE);
            return View(course);
        }

        #endregion

        #region DELETE

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        public ActionResult Delete(int id)
        {
            Course course = db.Courses.Single(c => c.Course_Id == id);
            return View(course);
        }

        [Authorize(Roles = "Super,BranchManager,Administrator")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Course course = db.Courses.Single(c => c.Course_Id == id);
            // Delete Template_Document
            var templateDocumentToDelete = db.Template_Document.Where(x => x.Course_Id.Equals(course.Course_Id));

            if (templateDocumentToDelete.Count() > 0)
            {
                foreach (Template_Document templateDocument in templateDocumentToDelete)
                {
                    db.Template_Document.DeleteObject(templateDocument);
                }
            }
            var applicationDocuments = (from app in db.Application_Document
                                        from temp in db.Template_Document
                                        where temp.Course_Id == id && temp.TemplateDoc_Id == app.TemplateDoc_Id
                                        select app);
            int appId = 0;
            if (applicationDocuments.Count() > 0)
            {
                foreach (Application_Document application_Document in applicationDocuments)
                {
                    try
                    {
                        appId = application_Document.Application_Id;
                        db.Application_Document.DeleteObject(application_Document);
                        ApplicationController.deleteAllApplications(appId);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        ModelState.AddModelError(String.Empty, "Cannot delete");
                        return View(course);
                    }
                }
            }

            var applications = db.Applications.Where(x => x.Course_Id == id);
            int appsId = 0;
            if (applications.Count() > 0)
            {
                foreach (Application application in applications)
                {
                    try
                    {
                        appsId = application.Application_Id;
                        db.Applications.DeleteObject(application);
                        ApplicationController.deleteAllApplications(appsId);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        ModelState.AddModelError(String.Empty, "Cannot delete");
                        return View(course);
                    }
                }
            }


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

        #region PRIVATE FUNCTION


        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}

        #endregion
    }
}