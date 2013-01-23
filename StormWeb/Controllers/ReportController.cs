﻿/* Author: Maysa Labadi
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
                else if (reportType == "Student Info")
                {
                    StudentInfoReport(viewModel.allTime, viewModel.dateTimeStart, viewModel.dateTimeEnd);
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

        private void MakeReport(ReportModel rm)
        {
            Response.BinaryWrite(rm.makeReport());
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=Report.xlsx");
        }

        public void StudentInfoReport(bool allTime, DateTime start, DateTime end)
        {
            List<Client> clients;

            if (allTime)
            {
                clients = db.Clients.ToList();
            }
            else
            {
                clients = db.Clients.Where(x => x.Registered_On >= start && x.Registered_On <= end).ToList();
            }

            string[] reportHeader = { "ID", "First Name", "Last Name", "Enrolled Institution", "Current Course", "Intake Enrolled", "DoB", "Contact Number", "Email", "Registered On" };

            ReportModel rm = new ReportModel(reportHeader);

            foreach (Client c in clients)
            {
                // Insert data as string array with content matching the header above
                int countCompleted = c.Students.FirstOrDefault().Applications.Where(x => x.Status == ApplicationController.ApplicationStatusType.CoE.ToString()).Count();

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
                        ((DateTime) c.Registered_On).ToString("dd-MM-yyyy")
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
                        ((DateTime) c.Registered_On).ToString("dd-MM-yyyy")
                    };

                    rm.addData(data);
                }

                
            }

            MakeReport(rm);
        }

        public void InvoiceReport(bool allTime, DateTime start, DateTime end)
        {
            List<Payment> pay;
            if (allTime)
            {
                pay = db.Payments.ToList();
            }
            else
            {
                pay = db.Payments.Where(x => x.Date_Of_Payment >= start && x.Date_Of_Payment <= end).ToList();
            }
            string[] reportHeader = { "Id", "Surname", "First Name", "Course", "Course Intake", "Student Payment" };

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
