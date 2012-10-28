using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class ClientViewModel
    {
        public Client ClientModel { get; set; }
        public Client_SkillTest SkillTestModel { get; set; }
        public Client_StudyServiceDetails StudyServiceDetailsModel { get; set; }
        public Client_Qualification QualificationsModel { get; set; }
        public Address AddressModel { get; set; } 
        public CountyList CountryList { get { return new CountyList(); }  }
        public Student StudentModel { get; set; }
        public Client_Spouse SpouseModel { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
    }
} 