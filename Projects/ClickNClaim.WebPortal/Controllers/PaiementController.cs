using ClickNClaim.Business;
using ClickNClaim.Common;
using ClickNClaim.WebPortal.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClickNClaim.WebPortal.Controllers
{
    public class PaiementController : Controller
    {
        // GET: Paiement
        public ActionResult Cancelled()
        {
            TempData["Error"] = "Vous avez annulé le paiement de votre demande d'arbitrage. Vous pouvez a tout moment la refaire en cliquant sur le bouton \"Demande d'arbitrage\" sur le résumé de votre litige.";
            if (Request.Cookies.AllKeys.Contains("conflictId"))
            {
                var conflictId = Request.Cookies["conflictId"].Value;
                int confId = 0;
                if (int.TryParse(conflictId, out confId))
                {
                    return RedirectToAction("Conflict", "Viewer", new { conflictId = confId });
                }
                else
                {
                    TempData["Error"] += "Vous retrouverez le litige concerné dans votre liste de litige ci-dessous.";
                    return RedirectToAction("Profil", "Account");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Confirm()
        {
            if (Request.Cookies.AllKeys.Contains("conflictId"))
            {
                var conflictId = Request.Cookies["conflictId"].Value;
                int confId = 0;
                if (int.TryParse(conflictId, out confId))
                {
                    BLLConflicts.UpdatePaiement(confId);

                    var conflict = BLLConflicts.GetConflictForUser(confId, User.Identity.GetId());
                    if (conflict.AskedForArbitration == null || !conflict.AskedForArbitration.Value)
                    {
                        conflict.AskedForArbitration = true;
                        conflict.State = (int)ConflictState.ArbitrationAsked;
                        BLLConflicts.UpdateConflict(conflict);
                    }

                    return RedirectToAction("PaymentDone", "Viewer", new { conflictId = confId });
                }
            }
            TempData["Error"] += "Nous n'avons pas réussi à suivre le paiement de votre demande d'arbitrage. Celui-ci devrait vous être confirmer par Paypal. Vous retrouverez le litige concerné dans votre liste de litige ci-dessous.";
            return RedirectToAction("Profil", "Account");

        }

    }
}