using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;


namespace StormWeb.Controllers
{
    public class ChartController : Controller
    {
        //
        // GET: /Chart/

        StormDBEntities db = new StormWeb.Models.StormDBEntities();

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult getStudentsChart()
        {
            DateTime start = DateTime.Now.AddMonths(-12);

            StudentChartModel newStudents = new StudentChartModel();
            
            // Number of clients registered in past 12 months
            var clients = db.Clients.Where(x => x.Registered_On >= start);

            for(int i = 0; i < 12; i++)
            {
                string dString = start.ToString("MM/yyyy");
                DateTime nextMonth = start.AddMonths(1);

                int val = clients.Where(x => x.Registered_On >= start && x.Registered_On <= nextMonth).Count();

                newStudents.add(val, dString);

                start = start.AddMonths(1);
            }


            string [] test = {"asdasd", "asdasd"};

            string json = newStudents.jsonSerializer();

            return Json(newStudents.jsonSerializer(), JsonRequestBehavior.AllowGet);
        }


        
    }
}
