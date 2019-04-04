using ClickNClaim.Business;
using ClickNClaim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClickNClaim.WebPortal.Extensions;

namespace ClickNClaim.WebPortal.Controllers
{
    [RequireHttps]
    [Authorize]
    public class ActionController : Controller
    {
        [ValidateInput(false)]
        public ContentResult AddAction(Disagreement disagreement)
        {
            disagreement.IdUser = User.Identity.GetId();
            return new ContentResult() { Content = JsonHelper.GetJsonString(BLLDisagreements.AddDisagreement(disagreement)), ContentType = "application/json" };
           // return new JsonResult() { Data = BLLDisagreements.AddDisagreement(disagreement), JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
        [ValidateInput(false)]
        public ContentResult AddDemandDisagreement(string disagreement, int idEvent, int? idDisagreement)
        {
            Disagreement d = new Disagreement();
            d.DisagreementOnDate = false;
            d.DisagreementOnDescription = true;
            d.DisagreementOnPiece = false;
            d.IdUser = User.Identity.GetId();
            d.Comment = disagreement;
            d.IdEvent = idEvent;
            if (idDisagreement != null)
                d.Id = idDisagreement.Value;

            return new ContentResult() { Content = JsonHelper.GetJsonString(BLLDisagreements.AddDisagreement(d)), ContentType = "application/json" };
        }


    }
}