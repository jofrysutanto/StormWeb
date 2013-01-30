using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Models
{
    [MetadataType(typeof(AssociateMD))]
    public partial class Associate
    {
    }

    public class AssociateMD
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string AssociateName { get; set; }

        [Required]
        public string ContactPerson { get; set; }

        [Required]
        public string Primary_Contact { get; set; }

        [Required]
        public string Secondary_Contact { get; set; }

        [Required]
        public string Email { get; set; }

     }
}