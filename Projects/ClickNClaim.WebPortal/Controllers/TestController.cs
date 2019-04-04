using ClickNClaim.Business;
using ClickNClaim.Common;
using ClickNClaim.WebPortal.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ClickNClaim.WebPortal.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public FileResult Bodacc()
        {
            var file = DocGenerator.GenerateBodaccReport("414819409");

            return File(file.FileStream, MimeMapping.GetMimeMapping("pdf"));

            //BodaccReader.BodaccReader br = new BodaccReader.BodaccReader();
            //var elts = br.GetBodacc("414819409");
            //return new ContentResult() { Content = JsonConvert.SerializeObject(elts), ContentType = "application/json" };
        }

        public ContentResult SendAllMails()
        {
            var to = "emmanuel.mouclier@legal-e-services.com";
            var fake = "http://fakeaddress.com";

            // FastArbitreEmails.AccountCreated(to, "test emails", "testEmails", fake, Guid.NewGuid());
            //FastArbitreEmails.AccountCreatedForUserByLawyer(to, "TestEmails", fake, fake, Guid.NewGuid());
            //FastArbitreEmails.ArbitrationAsked(to, 412, fake, fake, Guid.NewGuid());
            //FastArbitreEmails.CaseRefused(to, "La raison du refus apparait ici!", fake, Guid.NewGuid());
            //FastArbitreEmails.ConfirmAccount(to, "Test emails", fake, fake, Guid.NewGuid());
            FastArbitreEmails.InvitationToJoinConflict(new Common.Invitation() { Email = to }, new Common.Conflict() { HasArbitralClause = false, Id = 42, IdCreationUser = "42", UsersInConflicts = new List<UsersInConflict>() { new UsersInConflict() { UserDescriptionOfTheConflict = "Ma description", IdUser = "42" } } }, fake, "test emails", fake, Guid.NewGuid());
            //FastArbitreEmails.LawyerCalledOnCase(to, "lawyer name", 412, "test client name", fake, "testlogin", "testpassword", fake, Guid.NewGuid());
            //FastArbitreEmails.LawyerStartedCase(to, fake,412, fake, fake, Guid.NewGuid());
            //FastArbitreEmails.NewMissionOrder(to, fake, fake, Guid.NewGuid());
            //FastArbitreEmails.PreConclusion(to,"user", 442, fake, fake, fake, Guid.NewGuid());
            //FastArbitreEmails.ReinitiatingPassword(to, fake, fake, Guid.NewGuid());
            //FastArbitreEmails.Sentence(to, 442, fake, fake, fake, Guid.NewGuid());
            //FastArbitreEmails.VisioAsked(to, "test email", 412, "raison de la demande de visio", fake, "refus", fake, Guid.NewGuid());
            //FastArbitreEmails.VisioConferencePlanned(to, DateTime.Now, "titre visio", "description visio", fake, "login", "password", fake, Guid.NewGuid());
            //FastArbitreEmails.VisioMultipleDateChoice(to, "titre conf", 442, fake, fake, Guid.NewGuid());
            //FastArbitreEmails.VisioNotProgrammable(to, "titre conf", 442, fake, fake, Guid.NewGuid());
            //FastArbitreEmails.VisioProgrammed(to,412, "url", "titre conf", new Common.MeetingProposition() { DateTimeProposition = DateTime.Now }, fake, Guid.NewGuid());
            //FastArbitreEmails.VisioProgrammedForArbiter(to, "titre conf", 442, fake, new Common.MeetingProposition() { DateTimeProposition = DateTime.Now }, fake, Guid.NewGuid());
            //FastArbitreEmails.YouHaveMail(to, new Common.Debate() { Title = "titre débat" }, fake, fake, Guid.NewGuid());

            //Guid guid = Guid.NewGuid();
            //FastArbitreEmails.EndFreePhase("pierrelasvigne@hotmail.com", "Pierre Lasvigne", string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~")) + Url.Action("Conflict", "Viewer", new { conflictId = 808 }),
            //  Request.Url.Host.ToLower() + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
            //FastArbitreEmails.ConfirmContestationFilledIn("pierrelasvigne@hotmail.com", string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~")) + Url.Action("Conflict", "Viewer", new { conflictId = 808 }),
            //   Request.Url.Host.ToLower() + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

            return new ContentResult() { Content = "OK" };
        }


        public ActionResult ErrorTesting()
        {
            throw new Exception("Une erreur est survenue");
        }

        //public ContentResult Index()
        //{
        //    var file = DocGenerator.GenerateBodaccReport("414819409");

        //    return new ContentResult() { Content = file, ContentType = "text/html" };
        //}

        public ContentResult ConflictTest()
        {
          var conflicts =  BLLConflicts.GetConflictsWithInvitations();

            return new ContentResult() { Content = JsonConvert.SerializeObject(conflicts), ContentType = "application/json" };
        }


    }
}