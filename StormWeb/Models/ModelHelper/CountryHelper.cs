using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models.ModelHelper
{
    public class CountryHelper
    {
        public int CountryCode { get; set; }
        public string CountryName { get; set; }

        public static IQueryable<CountryHelper> GetCountries()
        {
            StormDBEntities db = new StormDBEntities();
            List<Country> countryList = db.Countries.ToList();
            List<CountryHelper> countryHelper = new List<CountryHelper>();

            foreach (Country c in countryList)
            {
                CountryHelper ch = new CountryHelper
                {
                    CountryCode = c.Country_Id,
                    CountryName = c.Country_Name
                };
                countryHelper.Add(ch);
            }

            return countryHelper.AsQueryable();
        }

        public static IQueryable<CountryHelper> GetRepresentedCountries()
        {
            return new List<CountryHelper>  
            {  
                new CountryHelper {  
                    CountryCode = 16,  
                    CountryName = "Australia"  
                },  
                new CountryHelper {  
                    CountryCode = 132,  
                    CountryName = "New Zealand"  
                }, 
                new CountryHelper {  
                    CountryCode = 189,  
                    CountryName = "United Kingdom"  
                }, 
                new CountryHelper {  
                    CountryCode = 190,  
                    CountryName = "United States"  
                }, 
                new CountryHelper{  
                    CountryCode = 38, 
                    CountryName = "Canada"  
                }  
            }.AsQueryable();  
        }
    }
}