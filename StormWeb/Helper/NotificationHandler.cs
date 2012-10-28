using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Helper
{
    public static class NotificationHandler
    {
        public static string type = String.Empty;
        public static string message = String.Empty;
        public static string title = String.Empty;

        public static bool hasNotification()
        {
           return message != String.Empty ? true : false;
        }

        public static string[] getNotification()
        {
            string [] result =  new string [] { type, message, title };
            clear();
            return result;
        }

        public static void clear()
        {
            type = String.Empty;
            message = String.Empty;
            title = String.Empty;
        }

        /// <summary>
        /// Set a notification message which will be shown at the next page.
        /// nType must be one of the NOTY_* type found under NotificationHandler
        /// </summary>
        /// <param name="nType"></param>
        /// <param name="nMessage"></param>
        public static void setNotification(string nType, string nMessage)
        {
            type = nType;
            message = nMessage;

            if (type == NOTY_SUCCESS)
            {
                title = "Success!";
            }
            else if (type == NOTY_ERROR)
            {
                title = "Error!";
            }
            else if (type == NOTY_WARNING)
            {
                title = "Alert!";
            }
            else if (type == NOTY_INFO)
            {
                title = "Info";
            }
        }

        #region Notification Types
        public static string NOTY_SUCCESS = "success";
        public static string NOTY_ERROR = "error";
        public static string NOTY_WARNING = "warning";
        public static string NOTY_INFO = "info";
        #endregion
    }
}