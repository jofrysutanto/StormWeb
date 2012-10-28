using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class ManageRoleModel
    {
        public string username { get; set; }
        public string[] rolesList { get; set; }
        public string[] assignedRolesList { get; set; }
    }
}