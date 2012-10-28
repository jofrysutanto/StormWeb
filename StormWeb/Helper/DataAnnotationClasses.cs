using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StormWeb.Helper
{
    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute()
            : base(GetRegex())
        { }

        private static string GetRegex()
        {
            // TODO: Go off and get your RegEx here
            return @"^[\w-]+(\.[\w-]+)*@([a-z0-9-]+(\.[a-z0-9-]+)*?\.[a-z]{2,6}|(\d{1,3}\.){3}\d{1,3})(:\d{4})?$";
        }
    }
}