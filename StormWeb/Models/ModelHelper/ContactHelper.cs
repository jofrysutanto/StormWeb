

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models.ModelHelper
{
    public class ContactHelper
    {
        public string Username { get; set; }
        public string Name { get; set; }

        // List out the contact details for particular user (used in sending messages
        // If currentRole == TRUE, Role is student
        // Else, Role is staff
        public static IQueryable<ContactHelper> GetContacts(bool currentRole, int currentID)
        {
            StormDBEntities db = new StormDBEntities();
            List<ContactHelper> contactList = new List<ContactHelper>();

            // If student, can only contact staff in charge of his/her case
            if (currentRole)
            {
                Case c = db.Cases.Single(x => x.Student_Id.Equals(currentID));

                var staffs = from cs in db.Case_Staff
                             from s in db.Staffs
                             where cs.Staff_Id == s.Staff_Id && cs.Case_Id == c.Case_Id
                             select s;

                //Staff counsellor = db.Staffs.Single( x => x.Staff_Id.Equals(c.Counsellor_Id));

                foreach (Staff s in staffs.ToList())
                {
                    ContactHelper contact = new ContactHelper
                    {
                        Username = s.UserName,
                        Name = "Staff - " + s.FirstName + " " + s.LastName
                    };
                    contactList.Add(contact);
                }


                
            }
            else
            {
                var staffs = db.Staffs.ToList();

                foreach (Staff s in staffs.ToList())
                {
                    ContactHelper contact = new ContactHelper
                    {
                        Username = s.UserName,
                        Name = s.FirstName + " " + s.LastName
                    };
                    contactList.Add(contact);
                }

                var students = from c in db.Cases
                            from cs in db.Case_Staff
                            from s in db.Students
                            where currentID == cs.Staff_Id && cs.Case_Id == c.Case_Id && c.Student_Id == s.Student_Id
                            select s;

                foreach (Student s in students.ToList())
                {
                    ContactHelper contact = new ContactHelper
                    {
                        Username = s.UserName,
                        Name = "Student - " + s.Client.GivenName + " " + s.Client.LastName
                    };
                    contactList.Add(contact);
                }
                            
            }

            return contactList.AsQueryable();           
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