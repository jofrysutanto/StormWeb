using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models.ModelHelper
{
    public class CourseLevelHelper
    {
        public string CourseLevel { get; set; }

        public static IQueryable<CourseLevelHelper> GetItems()
        {
            return new List<CourseLevelHelper>  
            {  
                new CourseLevelHelper {  
                    CourseLevel = "Diploma (D1/D2/D3)"
                },  
                new CourseLevelHelper {  
                    CourseLevel = "Bachelor"
                },  
                new CourseLevelHelper {  
                    CourseLevel = "Master"
                },
                new CourseLevelHelper {  
                    CourseLevel = "Post-Graduate/Doctoral"
                }
            }.AsQueryable();
        } 
    }
}