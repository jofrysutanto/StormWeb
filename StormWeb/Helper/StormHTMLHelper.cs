using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StormWeb.Helper
{
    public static class StormHTMLHelper
    {
        public static HtmlString SpinnerImg(this HtmlHelper helper, string alt)
        {
            return new HtmlString(String.Format("<img src=\"../../Content/img/ajax-loader.gif\" alt=\"{0}\"/>", alt));
        }

        public static MvcHtmlString Link_DownloadFile(this HtmlHelper helper, string innerHTML, string url)
        {
            return Link_DownloadFile(helper, innerHTML, url, null);
        }

        public static MvcHtmlString Button_Add(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-danger iframe");
            button.MergeAttribute("href", url);

            button.InnerHtml = innerHTML + " +";

            return MvcHtmlString.Create(button.ToString());
        }

        public static MvcHtmlString Link_DownloadFile(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var link = new TagBuilder("a");

            link = buildMyTag(htmlAttr, link);
            link.MergeAttribute("href", url);
            link.MergeAttribute("target", "_new");

            link.InnerHtml = "<i class=\"color-icons doc_pdf_co\"></i>" + innerHTML;

            //<a href="@Url.Action("DownloadTempDoc", new { id = x.TemplateDoc_Id })" target='_blank' class='tip-top' data-original-title="Download Template">
              //                                                  <i class="color-icons doc_pdf_co"></i>@Html.DisplayFor(modelItem => x.Form_Name)</a></li>

            return MvcHtmlString.Create(link.ToString());

        }

        private static TagBuilder buildMyTag(object htmlAttr, TagBuilder tagIn)
        {
            IDictionary<String, Object> dict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttr);

            tagIn.MergeAttributes(dict);

            return tagIn;
        }
    }
}