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
            
            ViewBag.Branch_Id = new SelectList(db.Branches, "Branch_Id", "Branch_Name");
            return View();
        }

        [HttpPost]
        public FileResult RunReport(FormCollection fc, int id = 1)
        {
            DateTime datefrom = DateTime.ParseExact(fc["Date_From"], "d", null);
            DateTime dateto = DateTime.ParseExact(fc["Date_To"], "d", null);
            List<Client> clients = db.Clients.Where(x => x.Branch_Id == id && datefrom <= x.Dob && x.Dob <= dateto).ToList();
            return ExportClientsCSV(clients);
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
                );
            }
            writer.Flush();
            output.Seek(0, SeekOrigin.Begin);
            
            return File(output, "text/csv", "Students_" + DateTime.Now.ToShortDateString().Replace('/', '-') + ".csv");
        }

        //
        // GET: /Report/Details/5
        public FileResult ExportStudentsCSV(IEnumerable<Student> students)
        {
            
            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output, System.Text.Encoding.UTF8);
            writer.WriteLine("Username,Year Level,School Name,State");
            foreach (Student student in students)
            {
                writer.WriteLine(
                            "\"" + student.Student_Id
                    + "\",\"" + student.Client_Id
                    + "\",\"" + student.Client.GivenName 
                    + "\",\"" + student.Course_Choice
                );
            }
            writer.Flush();
            output.Seek(0, SeekOrigin.Begin);
            //string file = Path.Combine(pathToCreate, "Students_" + DateTime.Now.ToShortDateString().Replace('/', '-'));
            return File(output, "text/csv", "Students_" + DateTime.Now.ToShortDateString().Replace('/', '-') + ".csv");
        }
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
