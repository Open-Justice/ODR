using ClickNClaim.WebPortal.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClickNClaim.WebPortal.Extensions;
using System.Web.Routing;

namespace ClickNClaim.WebPortal.Filters
{
    public class MailHandleError: HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            string errorFormatted = "";
            Guid guid = Guid.NewGuid();
            var replacements = new Dictionary<string, string>();
            var rd = new RouteValueDictionary();
            rd.Add("id", guid.ToString());
           
            replacements.Add("|*EMAILLINK*|", UrlHelper.GenerateUrl("", "Index", "Email", rd,RouteTable.Routes, filterContext.RequestContext, false));
            replacements.Add("|*USER*|", filterContext.HttpContext.User.Identity.IsAuthenticated ? filterContext.HttpContext.User.Identity.Name : "Anonymous");
            replacements.Add("|*DATE*|", DateTime.Now.ToLongDateString() + " à " + DateTime.Now.Hour + ":" + DateTime.Now.Minute);
            replacements.Add("|*URL*|",filterContext.HttpContext.Request.Url.AbsoluteUri);
            replacements.Add("|*BROWSER*|", filterContext.HttpContext.Request.Browser.Browser + "  version : " + filterContext.HttpContext.Request.Browser.Version);
            replacements.Add("|*ENCODING*|", filterContext.HttpContext.Request.ContentEncoding.BodyName);
            replacements.Add("|*FORMKEYS*|", filterContext.HttpContext.Request.Form.Join(c => String.Format("\"{0}\" : \"{1}\"<br/>", c, filterContext.HttpContext.Request.Form[c]), ","));
            replacements.Add("|*HEADERS*|", filterContext.HttpContext.Request.Headers.Join(c => String.Format("\"{0}\" : \"{1}\"<br/>", c, filterContext.HttpContext.Request.Headers[c]), ","));
            replacements.Add("|*QUERYSTRING*|", filterContext.HttpContext.Request.QueryString.Join(c => String.Format("\"{0}\" : \"{1}\"<br/>", c,filterContext.HttpContext.Request.QueryString[c]), ","));
            replacements.Add("|*EXCEPTION*|", filterContext.Exception.ToString());

            
            MailSender.SendMessage("pierrelasvigne@hotmail.com", "[ERROR][Fast Arbitre] Une exception est apparue!", MailSender.GetHtmlAndReplaceData("~/Emails/ErrorMail.html", replacements),guid);

            base.OnException(filterContext);
        }
    }
}