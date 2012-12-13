/* Author: Maysa Labadi
 * Date: 22/10/2012
 * 
 * Report Controller
 * 
 * Handles all required reports * 
 * 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using StormWeb.Models;
using StormWeb.Models.Report;

namespace StormWeb.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/
        private StormDBEntities db = new StormDBEntities();

        public ActionResult Index()
        {
            List<Student> students = db.Students.ToList();

            return View();
        }

        

        [HttpGet]
        public ViewResult RunReport()
        {
            //SelectList staff = new SelectList(staffName);
            //ViewBag.staffWithCases = staff.ToList();

            ViewBag.Branch_Id = new SelectList(db.Branches, "Branch_Id", "Branch_Name");

            ReportViewModel viewModel = new ReportViewModel();

            viewModel.allTime = true;

            return View(viewModel);
        }

        [HttpPost]
        public void RunReport(ReportViewModel viewModel)
        {

            Client c = new Client();

            Student s = new Student();

            if (!viewModel.allTime)
            {
                if (viewModel.dateTimeStart > viewModel.dateTimeEnd)
                {
                    ModelState.AddModelError("DateRangeError", new Exception("Starting date can not be greater than end date"));
                }
            }

            if (ModelState.IsValid)
            {
                string reportType = viewModel.reportType;

                if (reportType == "Sample Report")
                {
                    ReportSample();
                }
                else if (reportType == "Tax Invoice")
                {
                    ErrorReport();
                }
                else
                {
                    ErrorReport();
                }
            }
        }

        public void ReportSample()
        {
            DateTime lastYear = DateTime.Now.AddYears(-1);
            List<Application> apps = db.Applications.Where(x => x.Date_Of_ApplicationStatus >= lastYear).ToList();

            string[] reportHeader = { "Id", "Surname", "First Name", "Course", "Course Intake", "Student Payment" };

            ReportModel rm = new ReportModel(reportHeader);

            foreach (Application app in apps)
            {
                string s1 = Convert.ToString(app.Application_Id);
                string s2 = app.Case.Student.Client.LastName;
                string s3 = app.Case.Student.Client.GivenName;
                string s4 = app.Course.Course_Name;
                string s5 = Convert.ToString(app.Course.Commence_Date_Sem);
                string s6 = app.Payments.Count > 0 ? Convert.ToString(app.Payments.LastOrDefault().Amount) : "";

                string[] data = {
                    Convert.ToString(app.Application_Id),
                    app.Case.Student.Client.LastName,
                    app.Case.Student.Client.GivenName,
                    app.Course.Course_Name,
                    Convert.ToString(app.Course.Commence_Date_Sem),
                    app.Payments.Count > 0 ? Convert.ToString(app.Payments.LastOrDefault().Amount) : ""
                };

                rm.addData(data);
            }

            Response.BinaryWrite(rm.makeReport());
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=Report.xlsx");
        }

        public void ErrorReport()
        {
            ReportModel rm = new ReportModel(new string [] { "Error creating the report" });

            rm.addData(new string [] {"Please re-check your report parameter"});

            Response.BinaryWrite(rm.makeReport());
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=Report.xlsx");
        }
    }
}
