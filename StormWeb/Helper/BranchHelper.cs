using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StormWeb.Models;

namespace StormWeb.Helper
{
    public static class BranchHelper
    {
        private static StormDBEntities db = new StormDBEntities();

        // Return the branch name with given Branch ID
        public static string getBranchName(int id)
        {
            Branch b = db.Branches.Single(x => x.Branch_Id == id);
            return b.Branch_Name;
        }

        public static string getBranchNameList(int staffId)
        {
            // Also add branch where the staff belong                        
            var branches = from b in db.Branches
                           from bs in db.Branch_Staff
                           from s in db.Staffs
                           where s.Staff_Id == staffId && s.Staff_Id == bs.Staff_Id && b.Branch_Id == bs.Branch_Id
                           select b;

            int branchCount = branches.Count();

            if (branchCount == 0)
                return "";
            else if (branchCount == 1)
            {
                return branches.First().Branch_Name;
            }
            else
            {
                Branch[] bArr = branches.ToArray();

                string combinedBranches = Convert.ToString("<ul><li>" + bArr[0].Branch_Name + "</li>");
                for (int i = 1; i < bArr.Length; i++)
                {
                    combinedBranches += "<li>" + bArr[i].Branch_Name + "</li>";
                }

                combinedBranches += "</ul>";

                return combinedBranches;

            }
        }

        /// <summary>
        /// Return splitted BranchID Array (Array of Integer) given the Combined version with delimiter
        /// </summary>
        /// <param name="combinedID">Combined ID</param>
        /// <param name="delimiter">delimiter as a char (default is pipe symbol ('|')</param>
        /// <returns></returns>
        public static int[] getBranchIDArray(string combinedID, char delimiter = '|')
        {
            if (combinedID == "")
                return new int [0];

            int[] branchIDs = new int[combinedID.Split(delimiter).Length];
            int idx = 0;

            if (branchIDs.Length == 1)
                return new int [1] {  Convert.ToInt32(combinedID) };

            foreach (string s in combinedID.Split(delimiter))
            {
                branchIDs[idx] = Convert.ToInt32(s);
                idx++;
            }

            return branchIDs;
        }

        public static Branch[] getBranchArray(string combinedID)
        {
            if (combinedID == "")
                return new Branch[0];

            int[] branchIDs = getBranchIDArray(combinedID);

            var branches = from selectedBranch in branchIDs
                           from dbBranch in db.Branches
                           where selectedBranch == dbBranch.Branch_Id
                           select dbBranch;

            return branches.ToArray();
        }

        public static List<Branch> getBranchList(string combinedID)
        {
            int[] branchIDs = getBranchIDArray(combinedID);

            var branches = from selectedBranch in branchIDs
                           from dbBranch in db.Branches
                           where selectedBranch == dbBranch.Branch_Id
                           select dbBranch;

            return branches.ToList();
        }

        public static List<Branch> getBranchListFromCookie()
        {  
            return getBranchList(CookieHelper.AssignedBranch);
        }

        // Return list of branch name given concatenated Branch ID with delimiter specified
        public static string getBranchNameList(string combinedID, char delimiter)
        {
            if (combinedID == "")
                return "";
            int[] branchIDs = getBranchIDArray(combinedID, delimiter);            

            var branches = from selectedBranch in branchIDs
                    from dbBranch in db.Branches
                    where selectedBranch == dbBranch.Branch_Id
                    select dbBranch;

            if (branches.Count() == 0)
                return "";
            else if (branches.Count() == 1)
                return branches.Single().Branch_Name;           
            else
            {
                Branch[] bArr = branches.ToArray();

                string result = bArr[0].Branch_Id + ":" + bArr[0].Branch_Name + "<br/>";
                for (int i = 1; i < bArr.Length; i++)
                {
                    result += bArr[i].Branch_Id + ":" + bArr[i].Branch_Name + "<br/>";
                }                

                return result;
            }
        }
    }
}