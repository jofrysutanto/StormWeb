using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace StormWeb.Models.ModelHelper
{
    public class Currency
    {
        public string curr { get; set; }

        public Currency(string s)
        {
            curr = s;
        }

        public static IEnumerable<Currency> getCurrencyFromXml()
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(HttpContext.Current.Server.MapPath("~/App_Data/currency.xml"));

            XmlNodeList currencies = xmldoc.GetElementsByTagName("currency");

            List<Currency> listCurrency = new List<Currency>();
            foreach (XmlNode node in currencies)
            {
                string currency = node.InnerText;

                Currency c = new Currency(currency);

                listCurrency.Add((c));
            }

            return listCurrency.AsEnumerable();

        }
    }

}