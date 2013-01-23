using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Helper;
using System.Web.Security;

namespace StormWeb.Controllers
{
    public class HRController : Controller
    {
        //
        // GET: /HR/
        StormDBEntities db = new StormDBEntities();

        public ActionResult Index()
        {
            var q = from branches in BranchHelper.getBranchListFromCookie()
                    from staff in db.Staffs
                    from staff_branch in db.Branch_Staff
                    where staff_branch.Branch_Id == branches.Branch_Id && staff_branch.Staff_Id == staff.Staff_Id
                    select staff;

            return View(q.ToList());
        }

        public ActionResult ActivityLog(int id)
        {
            var L = from staffLog in db.Staff_Log
                    from staff in db.Staffs
                    where staff.Staff_Id == id && staff.UserName == staffLog.UserName
                    select staffLog;
            Staff staffs=db.Staffs.Single(x=>x.Staff_Id==id);
            Branch_Staff branch = db.Branch_Staff.Single(x => x.Staff_Id == id); 
            ViewBag.StaffName = staffs.FirstName + " " + staffs.LastName;
            ViewBag.StaffDepartment = staffs.Staff_Dept.Dept_Name;
            ViewBag.StaffBranch = branch.Branch.Branch_Name; 
            foreach (string s in Roles.GetRolesForUser(staffs.UserName))
            {
                ViewBag.StaffRole = s;                              
            }
           return View(L); 
        }

    }
}
