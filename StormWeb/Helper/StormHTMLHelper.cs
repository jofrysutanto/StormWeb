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

        public static MvcHtmlString Button_Add_iframe(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-info iframe");
            button.MergeAttribute("href", url);

            button.InnerHtml = innerHTML + " +";

            return MvcHtmlString.Create(button.ToString());
        }

        public static MvcHtmlString Button_Add_iframe_small(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-info iframe-small");
            button.MergeAttribute("href", url);

            button.InnerHtml = innerHTML + " +";

            return MvcHtmlString.Create(button.ToString());
        }

        public static MvcHtmlString Button_DeleteOrCancel(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-danger");
            button.MergeAttribute("href", url);

            button.InnerHtml = innerHTML;

            return MvcHtmlString.Create(button.ToString());
        }
        public static MvcHtmlString Button_SaveOrUpdate(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-primary");
            button.MergeAttribute("href", url);

            button.InnerHtml = innerHTML;

            return MvcHtmlString.Create(button.ToString());
        }
        public static MvcHtmlString Button_BackToList(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-info");
            button.MergeAttribute("href", url);

            button.InnerHtml = "<i class=\"color-icons arrow_undo_co\"></i>" + innerHTML;

            return MvcHtmlString.Create(button.ToString());
        }

        public static MvcHtmlString Button_Add(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-info");
            button.MergeAttribute("href", url);

            button.InnerHtml = "<i class=\"color-icons add_co\"></i>" + innerHTML;

            return MvcHtmlString.Create(button.ToString());
        }
        public static MvcHtmlString Button_Edit(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-info");
            button.MergeAttribute("href", url);

            button.InnerHtml = "<i class=\"color-icons pencil_co\"></i>" + innerHTML ;

            return MvcHtmlString.Create(button.ToString());
        }

        public static MvcHtmlString Button_Upload(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-success iframe-small");
            button.MergeAttribute("href", url);

            button.InnerHtml = "<i class=\"white-icons bended_arrow_up\"></i>" + innerHTML;           

            return MvcHtmlString.Create(button.ToString());
        }

        public static MvcHtmlString Button_Download(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var button = new TagBuilder("a");

            button = buildMyTag(htmlAttr, button);

            button.MergeAttribute("class", "btn btn-primary");
            button.MergeAttribute("href", url);
            button.MergeAttribute("target", "_new");

            button.InnerHtml = "<i class=\"white-icons bended_arrow_down\"></i>" + innerHTML;

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

        public static MvcHtmlString Link_UploadFile(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var link = new TagBuilder("a");

            link = buildMyTag(htmlAttr, link);

            link.MergeAttribute("class", "iframe-small");
            link.MergeAttribute("href", url);

            link.InnerHtml = "<i class=\"color-icons page_white_get_co\"></i>" + innerHTML;

            return MvcHtmlString.Create(link.ToString());
        }

        public static MvcHtmlString Link_Edit(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var link = new TagBuilder("a");

            link = buildMyTag(htmlAttr, link);

            link.MergeAttribute("href", url);

            string icon = "<i class='color-icons page_white_edit_co'></i>";

            link.InnerHtml = icon + innerHTML;

            return MvcHtmlString.Create(link.ToString());
        }

        public static MvcHtmlString Link_Delete(this HtmlHelper helper, string innerHTML, string url, object htmlAttr = null)
        {
            var link = new TagBuilder("a");

            link = buildMyTag(htmlAttr, link);

            link.MergeAttribute("href", url);

            string icon = "<i class='color-icons delete_co'></i>";

            link.InnerHtml = icon + innerHTML;

            return MvcHtmlString.Create(link.ToString());
        }

        public static MvcHtmlString Button_Delete(this HtmlHelper helper, string innerHTML, string url, string customIconString = "")
        {
            var link = new TagBuilder("a");

                        
            link.MergeAttribute("href", url);
            link.MergeAttribute("class", "confirmMe btn btn-danger");

            string icon = customIconString == "" ? "<i class='icon-trash icon-white'></i>" : customIconString;

            link.InnerHtml = icon + innerHTML;

            return MvcHtmlString.Create(link.ToString());
        }

        public static MvcHtmlString Button_Success_Disabled(this HtmlHelper helper, string innerHTML, string customIconString = "")
        {
            var link = new TagBuilder("a");


            link.MergeAttribute("href", "#");
            link.MergeAttribute("class", "btn btn-success disabled");
            link.MergeAttribute("style", "color:white");
            link.MergeAttribute("disabled", "disabled");

            string icon = customIconString == "" ? "" : customIconString;

            link.InnerHtml = icon + innerHTML;

            return MvcHtmlString.Create(link.ToString());
        }

        public static MvcHtmlString Button_Success(this HtmlHelper helper, string innerHTML, string url, string customIconString = "")
        {
            var link = new TagBuilder("a");

            link.MergeAttribute("href", url);
            link.MergeAttribute("class", "btn btn-success");

            string icon = customIconString == "" ? "" : customIconString;

            link.InnerHtml = icon + innerHTML;

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