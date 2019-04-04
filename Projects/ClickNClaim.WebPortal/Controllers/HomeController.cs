using ClickNClaim.Business;
using ClickNClaim.WebPortal.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ClickNClaim.WebPortal.Controllers
{
   //[RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [Route("Contact")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ContactUs(string Title, string Body) {
            MailSender.SendMessage("contact@fast-arbitre.com", Title, Body);
            return RedirectToAction("Index", "Home");
        }

        [Route("Pricing")]
        public ActionResult Pricing()
        {
            return View("Tarifs");
        }

        [Route("MentionsLegales")]
        public ActionResult MentionsLegales()
        {
            return View();
        }

        [Route("CharteEthique")]
        public ActionResult CharteEthique()
        {
            return View();
        }

        [Route("Clause")]
        public ActionResult ClauseCompromissoire()
        {
            return View("ClauseCompromissoire");
        }

        [Route("Comment-ca-marche")]
        public ActionResult HowItWorks()
        {
            return View();
        }

        [Route("Jobs")]
        public ActionResult Jobs()
        {
            return View("Jobs");
        }

        [Route("Privacy")]
        public ActionResult Privacy()
        {
            return View("Privacy");
        }
       
        public ActionResult ResendInvitation(string id)
        {
            var invit = BLLInvitations.GetInvitation(Guid.Parse(id));
            var conflict = BLLConflicts.GetConflict(invit.IdConflict);
            var guid = Guid.NewGuid();
            var url = Url.Action("Conflict", "Viewer", new { conflictId = invit.IdConflict });
            FastArbitreEmails.InvitationToJoinConflict(invit, conflict,
                   String.Format("{0}?i={1}", Request.Url.Authority.ToLower() + url, invit.Id),
                   conflict.CreatedBy.DisplayName, 
                   Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
            return View();
        }


    }
}