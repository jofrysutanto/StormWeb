<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.DateTime?>" %>  
<label>
<%: Html.TextBox("",  String.Format("{0:dd/MM/yyyy}", Model.HasValue ? Model : DateTime.Today), new { @class = "dp", @readonly="readonly"})%>
</label>