using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClickNClaim.WebPortal.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        public ActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View("AccessDenied");
        }

        public ActionResult UnknownError()
        {
            Response.StatusCode = 500;
            return View("UnknownError");
        }
    }
}