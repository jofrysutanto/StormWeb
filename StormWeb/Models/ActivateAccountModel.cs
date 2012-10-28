using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class ActivateAccountModel
    {
        public int staffId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
    }
}