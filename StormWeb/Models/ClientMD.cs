using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using StormWeb.Helper;

namespace StormWeb.Models
{
    [MetadataType(typeof(ClientMD))]
    public partial class Client
    {
        public class ClientMD
        {            
            [StringLength(50), Required]
            public string GivenName { get; set; }

            [StringLength(50), Required]
            public string Title { get; set; }

            [StringLength(50), Required]
            public string LastName { get; set; }

            [Email, Required]
            public string Email { get; set; }

            [StringLength(50), Required]
            public string ContactNumber { get; set; }

            [Required]
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy H:mm:ss}")]
            public string Dob { get; set; }
                
            //public object NoSuchProperty { get; set; }
        }
    }
}