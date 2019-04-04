using ClickNClaim.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;


namespace ClickNClaim.Common.Attributes
{
    public class ConflictNarrowerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
          

            if ((filterContext.ActionDescriptor.GetCustomAttributes(true).Any(c => c is ConflictNarrowerAttribute) ||
                filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(true).Any(c => c is ConflictNarrowerAttribute)) &&
                !filterContext.ActionDescriptor.GetCustomAttributes(true).Any(c => c is AllowAnonymousAttribute))
            {
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Account", action = "Login" }
                    ));
                }

                var userId = filterContext.HttpContext.User.Identity.GetUserId();



                var conflictId = int.Parse(filterContext.ActionParameters["conflictId"].ToString());

               if (!BLLConflicts.IsUserInConflict(conflictId, HttpContext.Current.User.Identity.GetUserId()))
                {
                    filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(
                   new { controller = "Error", action = "AccessDenied" }
               ));
                }

               if(BLLConflicts.GetConflictState(conflictId) >= ConflictState.Open)
                {
                    filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(
                  new { controller = "Viewer", action = "Conflict", routeValues = new { conflictId = conflictId } }));
                }


            }



            base.OnActionExecuting(filterContext);
        }
    }
}
