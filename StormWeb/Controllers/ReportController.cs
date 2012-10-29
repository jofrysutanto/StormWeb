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
            return View();
        }

        [HttpPost]
        public FileResult RunReport(FormCollection fc)
        {
            int branchId = Convert.ToInt32(fc["Branch_Id"]);
            DateTime datefrom = DateTime.ParseExact(fc["Date_From"], "d", null);
            DateTime dateto = DateTime.ParseExact(fc["Date_To"], "d", null);
           
            if (fc["report"] == "Option1")
            {
            List<Branch_Staff> staffs = db.Branch_Staff.Where(x => x.Branch_Id == branchId && x.Staff.Date_Of_Joining >= datefrom  && x.Staff.Date_Of_Joining <= dateto).ToList();
            return ExportStaffsCSV(staffs);
            }
            else
            {
                List<Client> clients = db.Clients.Where(x => x.Branch_Id == branchId && datefrom <= x.Dob && x.Dob <= dateto).ToList();
                return ExportClientsCSV(clients);
            }

        }

        public  FileResult ExportStaffsCSV(IEnumerable<Branch_Staff> bstaffs)
        {
            
            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output, System.Text.Encoding.UTF8);
            writer.WriteLine("Username,Date_Of_Joining,DOB,Email");
            foreach (Branch_Staff bstaff in bstaffs)
            {
                writer.WriteLine(
                            "\"" + bstaff.Staff.FirstName + " " + bstaff.Staff.LastName
                    + "\",\"" + bstaff.Staff.Date_Of_Joining
                    + "\",\"" + bstaff.Staff.DOB
                    + "\",\"" + bstaff.Staff.Email
                     + "\""
                );
            }
            writer.Flush();
            output.Seek(0, SeekOrigin.Begin);
            //string file = Path.Combine(pathToCreate, "Students_" + DateTime.Now.ToShortDateString().Replace('/', '-'));
            return File(output, "text/csv", "Staffs_" + DateTime.Now.ToShortDateString().Replace('/', '-') + ".csv");
        }

        public FileResult ExportClientsCSV(IEnumerable<Client> clients)
        {
            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output, System.Text.Encoding.UTF8);
            writer.WriteLine("Title,Username,email,nationality, D.O.B");
            foreach (Client client in clients)
            {
                writer.WriteLine(
                        "\"" + client.Title
                    + "\",\""  + client.GivenName + ' ' + client.LastName
                    + "\",\"" + client.Email
                    + "\",\"" + client.Nationality
                    + "\",\"" + client.Dob.ToShortDateString()
                     + "\""
                );
            }
            writer.Flush();
            output.Seek(0, SeekOrigin.Begin);
            
            return File(output, "text/csv", "Students_" + DateTime.Now.ToShortDateString().Replace('/', '-') + ".csv");
        }

        //
        // GET: /Report/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Report/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Report/Create

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
        // GET: /Report/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Report/Edit/5

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

        //
        // GET: /Report/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Report/Delete/5

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
