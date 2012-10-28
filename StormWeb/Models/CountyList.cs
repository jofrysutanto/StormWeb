using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace StormWeb.Models
{
    public class CountyList
    {
        public string SelectedCountry { get; set; }

        public IEnumerable<SelectListItem> Countries
        {
            get
            {
                return new[]
                {
                    new SelectListItem { Value = "Australia", Text = "Australia" },
                    new SelectListItem { Value = "USA", Text = "USA" },
                    new SelectListItem { Value = "Canada", Text = "Canada" }
                };
            }
        } 
    }
}