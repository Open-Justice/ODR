using ClickNClaim.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClickNClaim.WebPortal.Controllers
{
    public class EmailController : Controller
    {
        // GET: Email
        [Route("Email/Index/{id}")]
        public ActionResult Index(Guid id) 
        {
           var email = BLLEmails.GetEmail(id);

            return Content(email.body);
        }
    }
}