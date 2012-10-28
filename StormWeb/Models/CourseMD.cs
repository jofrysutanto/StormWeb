using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Models
{
    [MetadataType(typeof(CourseMD))]
    public partial class Course
    {
    }
    public class CourseMD
    {
        [Required(ErrorMessage = "Please enter course name")]
        public string Course_Name { get; set; }
    }
}