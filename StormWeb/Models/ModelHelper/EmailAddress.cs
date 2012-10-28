using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Models.ModelHelper
{    
    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute()
            : base(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}")
        {
            this.ErrorMessage = "Please provide a valid email address";
        }
    }

}


