using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Models
{
    [MetadataType(typeof(StaffMD))]
    public partial class Staff
    {
    }

    public class StaffMD
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}",ApplyFormatInEditMode = true)]
        [Required]
        public DateTime DOB { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Required]
        public DateTime Date_Of_Joining { get; set; }

        [Required]
        public int Dept_Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public int Mobile_Number { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }
    }
}