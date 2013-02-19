using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using StormWeb.Models.ModelHelper;

namespace StormWeb.Models
{
    [MetadataType(typeof(ResumeModel))]
    public partial class Resume
    {
    }
    public class ResumeModel
    {
        public string Resume_Id { get; set; }

        [Required(ErrorMessage = "Please enter Given Name")]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string GivenName { get; set; }

        [Required(ErrorMessage = "Please enter Last Name")]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter Address")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public int Address_Id { get; set; }

        [Required(ErrorMessage = "Please enter Contact Number")]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Please enter Secondary Contact number")]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string SecondaryContactNumber { get; set; }

        [Email(ErrorMessage = "Please enter valid EmailAddress"), Required(ErrorMessage = "Please enter EmailAddress")]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter Description")]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string Description { get; set; }
    }
}