using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models.ModelHelper
{
    public class TimeHelper
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public string AMPM { get; set; }

        public static IQueryable<TimeHelper> GetHours()
        {
            List<TimeHelper> tm = new List<TimeHelper>();
            for(int i=9;i<17;i++)
            {
                tm.Add(new TimeHelper{
                    Hours = i
                });
            }
            return tm.AsQueryable();
        }

        public static IQueryable<TimeHelper> GetMinutes()
        {
            List<TimeHelper> tm = new List<TimeHelper>();
            for (int i = 0; i < 60; i+=5)
            {
                tm.Add(new TimeHelper
                {
                    Minutes = i
                });
            }
            return tm.AsQueryable();
        }

        public static IQueryable<TimeHelper> GetAMPM()
        {
            return new List<TimeHelper>         
            {
            new TimeHelper{
                
                    AMPM = "AM"
                },
           new TimeHelper{
           
                    AMPM ="PM"
           }    
           }.AsQueryable();
        }
    }
}