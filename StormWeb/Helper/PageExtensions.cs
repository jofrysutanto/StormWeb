using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StormWeb.Helper
{
    public static class PageExtensions
    {
        public static void AddValidationSummaryItem(this Page page, string errorMessage)
        {
            var validator = new CustomValidator();
            validator.IsValid = false;
            validator.ErrorMessage = errorMessage;
            page.Validators.Add(validator);
        }
    } 
}