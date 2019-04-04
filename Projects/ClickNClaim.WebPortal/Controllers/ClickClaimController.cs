using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClickNClaim.WebPortal.Controllers
{
    public class ClickClaimController : Controller
    {
        [RequireHttps]
        // GET: ClickClaim
        public ActionResult Index()
        {
            return View();
        }
         
        [Route("Team")]
        public ActionResult Team()
        {
            return View();
        }

        [Route("ReglementArbitrage")]
        public ActionResult ReglementArbitrage()
        {
            return View();
        }

        [Route("CGU")]
        public ActionResult CGU()
        {
            return View();
        }
    }
}