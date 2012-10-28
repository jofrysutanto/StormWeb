using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models.ModelHelper
{
    public class NationalityHelper
    {
        public int CountryCode { get; set; }
        public string Nationality { get; set; }

        public static IQueryable<NationalityHelper> GetCountries()
        {
            StormDBEntities db = new StormDBEntities();
            List<Country> countryList = db.Countries.ToList();
            List<NationalityHelper> nationalHelper = new List<NationalityHelper>();

            foreach (Country c in countryList)
            {
                NationalityHelper ch = new NationalityHelper
                {
                    CountryCode = c.Country_Id,
                    Nationality = c.Nationality
                };
                nationalHelper.Add(ch);
            }

            return nationalHelper.AsQueryable();
        }
    }
}