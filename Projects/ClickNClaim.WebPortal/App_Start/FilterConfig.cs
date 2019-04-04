using ClickNClaim.Common.Attributes;
using ClickNClaim.WebPortal.Filters;
using System.Web;
using System.Web.Mvc;

namespace ClickNClaim.WebPortal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new MailHandleError());
            filters.Add(new ConflictNarrowerAttribute());
        }
    }
}
