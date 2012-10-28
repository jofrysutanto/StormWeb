using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class ResetPasswordModel
    {
        public string tempPassword { get; set; }
        public string newPassword { get; set; }
        public string username { get; set; }
        public string secretCode { get; set; }

        public ResetPasswordModel(string tempPassword, string username, string secretCode)
        {
            this.tempPassword = tempPassword;
            this.username = username;
            this.secretCode = secretCode;
        }
    }
}