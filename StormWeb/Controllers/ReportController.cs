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
            ViewBag.AssociateId = new SelectList(db.Associates, "AssociateId", "AssociateName");
            ViewBag.Location = new SelectList(db.Countries, "Country_Id", "Country_Name");
            ViewBag.University = new SelectList(db.Universities, "University_Id", "University_Name");

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
                   // ErrorReport();
                    TaxInvoice(viewModel.allTime, viewModel.dateTimeStart, viewModel.dateTimeEnd, viewModel);
                }
                else if (reportType == "Student Info")
                {
                    StudentInfoReport(viewModel.allTime, viewModel.dateTimeStart, viewModel.dateTimeEnd, viewModel);
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

            // Header for the report
            string[] reportHeader = { "Id", "Surname", "First Name", "Course", "Course Intake", "Student Payment" };

            ReportModel rm = new ReportModel(reportHeader);

            foreach (Application app in apps)
            {                
                // Insert data as string array with content matching the header above
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

            MakeReport(rm);
        }

        public void TaxInvoice(bool allTime, DateTime start, DateTime end, ReportViewModel viewModel)
        {
            DateTime lastYear = DateTime.Now.AddYears(-1);
            List<Application> apps = db.Applications.Where(x => x.Date_Of_ApplicationStatus >= lastYear).ToList();

            // Header for the report
            string[] reportHeader = { "Id", "Surname", "First Name", "Course", "Start Date", "Student Payment" ,"University Commission rate","Commission payable","Comments"};

            ReportModel rm = new ReportModel(reportHeader);

            foreach (Application app in apps)
            {
                // Insert data as string array with content matching the header above
               
                string[] data = {
                    Convert.ToString(app.Student.Client_Id),
                    app.Case.Student.Client.LastName,
                    app.Case.Student.Client.GivenName,
                    app.Course.Course_Name,
                    Convert.ToString(app.Course.Commence_Date_Sem),
                    app.Payments.Count > 0 ? Convert.ToString(app.Payments.LastOrDefault().Amount) : "",
                    Convert.ToString(app.Course.Faculty.University.Comission_Rate),
                    Convert.ToString((app.Payments.Count > 0? app.Payments.LastOrDefault().Amount:0)*(app.Course.Faculty.University.Comission_Rate/100)),
                    ""
                };

                rm.addData(data);
            }

            MakeReport(rm);
        }

        public void StudentInfoReport(bool allTime, DateTime start, DateTime end, ReportViewModel viewModel)
        {
            var clients = db.Clients.AsQueryable();

            if (!allTime)
            {
                clients = clients.Where(x => x.Registered_On >= start && x.Registered_On <= end);
            }

            if (viewModel.Branch > 0)
            {
                clients = clients.Where(x => x.Branch_Id == (int)viewModel.Branch);
            }

            if (viewModel.Associate > 0)
            {
                clients = clients.Where(x => x.Associate_Id == (int)viewModel.Associate);
            }

            if (viewModel.Location > 0)
            {
                clients = clients.Where(x => x.Address.Country_Id == (int)viewModel.Location);
            }

            if (viewModel.University > 0)
            {
                clients = from c in clients
                          from s in db.Students
                          from app in db.Applications
                          where c.Client_Id == s.Client_Id && app.Student_Id == s.Student_Id
                          select c;
            }

            string[] reportHeader = { "ID", "First Name", "Last Name", "Enrolled Institution", "Current Course", "Intake Enrolled", "DoB", "Contact Number", "Email", "Registered On", "Branch", "Associate", "Location" };

            ReportModel rm = new ReportModel(reportHeader);

            foreach (Client c in clients)
            {
                // Insert data as string array with content matching the header above
                int countCompleted = c.Students.FirstOrDefault().Applications.Where(x => x.Status == ApplicationController.ApplicationStatusType.CoE.ToString()).Count();

                string associateName = "---";

                if (c.Associate != null)
                    associateName = c.Associate.AssociateName;

                if (countCompleted > 0)
                {
                    StormWeb.Models.Application enrolledApplication = c.Students.FirstOrDefault().Applications.Single(x => x.Status == ApplicationController.ApplicationStatusType.CoE.ToString());

                    string[] data = {
                        Convert.ToString(c.Client_Id),
                        c.GivenName,
                        c.LastName,
                        enrolledApplication.Course.Faculty.University.University_Name,
                        enrolledApplication.Course.Course_Name,
                        ((DateTime) enrolledApplication.Course.Commence_Date_Sem).ToString("MM/yyyy"),
                        c.Dob.ToString("dd-MM-yyyy"),
                        c.ContactNumber,
                        c.Email,
                        ((DateTime) c.Registered_On).ToString("dd-MM-yyyy"),
                        c.Branch.Branch_Name,
                        associateName,
                        c.Address.Country.Country_Name
                    };

                    rm.addData(data);
                }

                if (countCompleted <= 0)
                {
                    string[] data = {
                        Convert.ToString(c.Client_Id),
                        c.GivenName,
                        c.LastName,
                        "---",
                        "---",
                        "---",
                        c.Dob.ToString("dd-MM-yyyy"),
                        c.ContactNumber,
                        c.Email,
                        ((DateTime) c.Registered_On).ToString("dd-MM-yyyy"),
                        c.Branch.Branch_Name,
                        associateName,
                        c.Address.Country.Country_Name
                    };

                    rm.addData(data);
                }


            }

            MakeReport(rm);
        }

        private void MakeReport(ReportModel rm)
        {
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
