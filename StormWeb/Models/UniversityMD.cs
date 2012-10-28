using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using StormWeb.Models.ModelHelper;

namespace StormWeb.Models
{
    [MetadataType(typeof(UniversityMD))]
    public partial class University
    {
    }
    public class UniversityMD
    {   
        public string University_Id { get; set; }

        [Required(ErrorMessage = "Please select Country")]
        public string Country_Id { get; set; }

        [Required(ErrorMessage="Please enter University")]
        public string University_Name { get; set; }

        [Required(ErrorMessage="Please enter Campus")]
        public string Campus { get; set; }

        [Email(ErrorMessage="Please enter valid EmailAddress"), Required(ErrorMessage="Please enter EmailAddress")]
        public string Email { get; set; }
    }
}