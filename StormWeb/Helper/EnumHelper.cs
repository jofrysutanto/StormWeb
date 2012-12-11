using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;

namespace StormWeb.Helper
{
    public class Enumclass
    {
        public enum Title
        {
            Mr = 1,
            Mrs = 2,
            Miss = 3,
            Ms = 4,
            Others = 5
        }

        public enum MaritalStatus
        {
            Single = 1,
            Married = 2,
            Divorced = 3,
            Widowed = 4
        }

        public enum Gender
        {
            Male = 'M',
            Female = 'F'
        }

        public enum Currency
        {
            INR = 0,
            AUD = 1,
            LKR =2,
            USD =3,

        }

        public virtual IList<SelectListItem> GetCurrency()
        {
            var names = Enum.GetNames(typeof(Enumclass.Currency));
            var values = Enum.GetNames(typeof(Enumclass.Currency));
            var titleItems = new List<SelectListItem>();
            for (var i = 0; i < names.Length; i++)
            {
                var val = values.GetValue(i);
                titleItems.Add(new SelectListItem { Text = names[i], Value = val.ToString() });
            }
            titleItems.Insert(0, new SelectListItem { Text = "--Select--", Value = "--Select--" });
            return titleItems;
        }

        public virtual IList<SelectListItem> GetTitle()
        {
            var names = Enum.GetNames(typeof(Enumclass.Title));
            var values = Enum.GetNames(typeof(Enumclass.Title));
            var titleItems = new List<SelectListItem>();
            for (var i = 0; i < names.Length; i++)
            {
                var val = values.GetValue(i);
                titleItems.Add(new SelectListItem { Text = names[i], Value = val.ToString() });
            }
            titleItems.Insert(0, new SelectListItem { Text = "--Select--", Value = "--Select--" });
            return titleItems;
        }

        public virtual IList<SelectListItem> GetMaritalStatus()
        {
            var names = Enum.GetNames(typeof(Enumclass.MaritalStatus));
            var values = Enum.GetNames(typeof(Enumclass.MaritalStatus));
            var maritalStatus = new List<SelectListItem>();
            for (var i = 0; i < names.Length; i++)
            {
                var val = values.GetValue(i);
                maritalStatus.Add(new SelectListItem { Text = names[i], Value = val.ToString() });
            }
            maritalStatus.Insert(0, new SelectListItem { Text = "--Select--", Value = "--Select--" });
            return maritalStatus;
        }

        public virtual IList<SelectListItem> GetGender()
        {
            var names = Enum.GetNames(typeof(Enumclass.Gender));
            var values = Enum.GetValues(typeof(Enumclass.Gender));
            var gender = new List<SelectListItem>();
            for (var i = 0; i < names.Length; i++)
            {
                var val = values.GetValue(i).ToString().Substring(0, 1);
                gender.Add(new SelectListItem { Text = names[i], Value = val.ToString() });
            }
            gender.Insert(0, new SelectListItem { Text = "--Select--", Value = "--Select--", Selected = values.Equals(values) });
            return gender;
        }

    }
}