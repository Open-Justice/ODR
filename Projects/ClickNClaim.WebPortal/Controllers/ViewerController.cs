using ClickNClaim.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClickNClaim.WebPortal.Extensions;
using ClickNClaim.Common;
using ClickNClaim.WebPortal.Tools;
using ClickNClaim.WebPortal.Models;
using ClickNClaim.WebPortal.Helpers;
using ClickNClaim.Common.Enums;

namespace ClickNClaim.WebPortal.Controllers
{
   // [RequireHttps]
    public class ViewerController : Controller
    {
        [Route("{conflictId}/Resume-de-mon-conflit")]
        // GET: Viewer
        public ActionResult Conflict(int conflictId, string i)
        {

            if (TempData.Keys.Any(c => c == "Info"))
            {
                ViewBag.Info = TempData["Info"];
            }
            else if (TempData.Keys.Any(c => c == "Error"))
            {
                ViewBag.Error = TempData["Error"];
            }
            else if (TempData.Keys.Any(c => c == "Success"))
            {
                ViewBag.Success = TempData["Success"];
            }

            var conflict = BLLConflicts.GetConflictForArbitration(conflictId);

            if (conflict.State == (int)ConflictState.FreeHandsArbitration)
            {
                var state = conflict.ConflictStateHistorics.Where(c => c.ConflictStateId == (int)ConflictState.FreeHandsArbitration).LastOrDefault();
                if (state != null)
                {
                    if (state.CreateDate.AddDays(state.CountDown.Value) < DateTime.Now)
                    {
                        BLLConflicts.AddConflictStateHistoric(new ConflictStateHistoric()
                        {
                            CreateDate = state.CreateDate.AddDays(state.CountDown.Value),
                            IdConflict = conflictId,
                            ConflictStateId = (int)ConflictState.ExchangeClosure,
                            ConflictStateName = MetadataHelper.GetEnumDisplayNAme(ConflictState.ExchangeClosure)
                        });

                        BLLConflicts.UpdateConflictState(conflictId, (int)ConflictState.ExchangeClosure);
                        conflict = BLLConflicts.GetConflictForArbitration(conflictId);
                    }
                }
            }
            else if (conflict.State == (int)ConflictState.PreConclusionDebate)
            {
                var state = conflict.ConflictStateHistorics.Where(c => c.ConflictStateId == (int)ConflictState.PreConclusionDebate).FirstOrDefault();
                if (state != null)
                {
                    if (state.CreateDate.AddDays(state.CountDown.Value) < DateTime.Now)
                    {
                        BLLConflicts.AddConflictStateHistoric(new ConflictStateHistoric()
                        {
                            CreateDate = state.CreateDate.AddDays(state.CountDown.Value),
                            IdConflict = conflictId,
                            ConflictStateId = (int)ConflictState.EndOfDebates,
                            ConflictStateName = MetadataHelper.GetEnumDisplayNAme(ConflictState.EndOfDebates)
                        });

                        BLLConflicts.AddConflictStateHistoric(new ConflictStateHistoric()
                        {
                            CreateDate = DateTime.Now,
                            IdConflict = conflictId,
                            ConflictStateId = (int)ConflictState.FinalDeliberation,
                            ConflictStateName = MetadataHelper.GetEnumDisplayNAme(ConflictState.FinalDeliberation)
                        });

                        BLLConflicts.UpdateConflictState(conflictId, (int)ConflictState.FinalDeliberation);
                        conflict = BLLConflicts.GetConflictForArbitration(conflictId);
                    }
                }
            }

            return View(conflict);
        }

        [Authorize]
        public ActionResult AskArbitration(int conflictId)
        {
            AskArbitrationViewModel model = new AskArbitrationViewModel();
            model.Conflict = BLLConflicts.GetConflictForArbitration(conflictId);
            model.User = BLLUsers.GetMyProfil(User.Identity.GetId());
            if (model.Conflict.UsersInConflicts.First(c => c.IdUser == model.User.Id).IdUserCompany != null)
                model.Company = BLLCompanies.GetCompany(model.Conflict.UsersInConflicts.First(c => c.IdUser == model.User.Id).IdUserCompany.Value);
            if (model.Company == null)
            {
                model.Company = new Company();
            }
            return View(model);
        }

        public ActionResult AskArbitrationValidation(int conflictId, Company company)
        {
            var conflict = BLLConflicts.GetConflictForUser(conflictId, User.Identity.GetId());


            var com = BLLUsers.AddOrUpdateCompany(company);

            BLLUsers.UpdateUserMainCompany(com.Id, User.Identity.GetId());

            var uic = conflict.UsersInConflicts.Where(c => c.IdUser == User.Identity.GetId()).FirstOrDefault();
            uic.HasAcceptedArbitration = true;
            BLLConflicts.UpdateUserInConflict(uic);

            conflict = BLLConflicts.GetFullConflict(conflictId);
            if (conflict.UsersInConflicts.All(c => c.HasAcceptedArbitration != null && c.HasAcceptedArbitration.Value) ||
                conflict.HasArbitralClause && conflict.UsersInConflicts.Any(c => c.HasAcceptedArbitration != null && c.HasAcceptedArbitration.Value) &&
                conflict.PaymentState == (int)PaymentStateEnum.Complete)
            {
                conflict.State = (int)ConflictState.ArbitrationReady;
                BLLConflicts.UpdateConflict(conflict);

                foreach (var item in conflict.UsersInConflicts)
                {
                    var hasPayed = false;
                    if (!hasPayed && (item.HasAcceptedArbitration == null || !item.HasAcceptedArbitration.Value))
                    {
                        Guid guid = Guid.NewGuid();
                        FastArbitreEmails.ArbitrationAsked(item.User.Email, conflict.Id,
                            Request.UrlReferrer.DnsSafeHost + Url.Action("AskArbitration", "Viewer", new { conflictId = conflict.Id }),
                             Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                        hasPayed = true;
                    }
                    else
                    {

                        Guid guid = Guid.NewGuid();
                        FastArbitreEmails.ArbitrationSentToCentre(item.User.Email, conflict.Id,
                            Request.UrlReferrer.DnsSafeHost + Url.Action("AskArbitration", "Viewer", new { conflictId = conflict.Id }),
                             Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                    }
                }
            }
            else
            {
                foreach (var item in conflict.UsersInConflicts.Where(c => c.HasAcceptedArbitration == null || !c.HasAcceptedArbitration.Value))
                {
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.ArbitrationAsked(item.User.Email, conflict.Id,
                        Request.UrlReferrer.DnsSafeHost + Url.Action("AskArbitration", "Viewer", new { conflictId = conflict.Id }),
                         Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                }
            }
            return RedirectToAction("Conflict", new { conflictId = conflictId });
        }

        //[Authorize]
        public ContentResult SearchCompany(string siren)
        {
            var template = "https://firmapi.com/api/v1/companies/{0}?api_key=5cad63ea1e284b706bbf02487d500e453e1d1003";

            try
            {
                WebClient wc = new WebClient();
                var res = wc.DownloadString(String.Format(template, siren));
                return new ContentResult() { Content = res, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult() { Content = "Le SIREN ne correspond a aucune entreprise chez nous.", ContentType = "application/json" };
            }



        }

        public ActionResult UpdateState(int conflictId, int newStateId, bool hasCountdown, int countdown)
        {
            var csh = new ConflictStateHistoric()
            {
                IdConflict = conflictId,
                ConflictStateId = newStateId,
                ConflictStateName = MetadataHelper.GetEnumDisplayNAme((ConflictState)newStateId),

            };
            if (hasCountdown)
                csh.CountDown = countdown;
            BLLConflicts.AddConflictStateHistoric(csh);
            BLLConflicts.UpdateConflictState(conflictId, newStateId);
            return RedirectToAction("Conflict", new { conflictId = conflictId });
        }

        [HttpPost]
        public ActionResult AskForVisio(int conflictId, string reason)
        {
            var conflict = BLLConflicts.GetConflictForArbitration(conflictId);
            Guid guid = Guid.NewGuid();
            FastArbitreEmails.VisioAsked(conflict.Arbiter.Email, User.Identity.FirstName() + " " + User.Identity.LastName(), conflictId, reason,
               Request.UrlReferrer.DnsSafeHost + Url.Action("Conflict", "Viewer", new { conflictId = conflictId }), Request.UrlReferrer.DnsSafeHost + Url.Action("RefuseVisio", new { conflictId = conflictId }),
                 Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

            TempData["info"] = "Votre message vient d'être envoyé a l'arbitre. Celui vous proposera rapidement des dates s'il accepte le motif de votre visio conférence.";

            return RedirectToAction("Conflict", new { conflictId = conflictId });
        }

        public ActionResult RefuseVisio(int conflictId)
        {
            return View(conflictId);
        }

        public ActionResult Quotation(int conflictId)
        {
            var conflict = BLLConflicts.GetConflict(conflictId);
            conflict.State = (int)ConflictState.Quotation;
            BLLConflicts.UpdateConflict(conflict);
            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }
        [Route("Cancelled")]
        public ActionResult Cancelled()
        {
            var conflictId = Request.Cookies["conflictId"].Value;
            int confId = 0;
            if (int.TryParse(conflictId, out confId))
            {
                return RedirectToAction("AsArbitration", new { conflictId = confId });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult PaymentDone()
        {
            var conflictId = Request.Cookies["conflictId"].Value;
            int confId = 0;
            if (int.TryParse(conflictId, out confId))
            {
                return View("PaymentDone", BLLConflicts.GetConflict(confId));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        public ActionResult UpdateCompanyInfo(int conflictId, string userId, Company company)
        {
            var c = BLLCompanies.AddOrUpdateCompany(company, userId);

            BLLCompanies.AddOrUpdateCompanyForUserInConflict(c.Id, userId, conflictId);

            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });

        }

        public FileResult GetBodaccReport(string siret)
        {
            var file = DocGenerator.GenerateBodaccReport(siret);
            return File(file.FileStream, "application/pdf", "RapportBodacc-" + siret + ".pdf");
        }


        public ContentResult AddNewEvent(Event e)
        {
            if (e.Id != 0)
            {
                return new ContentResult() { Content = JsonHelper.GetJsonString(BLLConflicts.UpdateEvent(e)), ContentType = "application/json" };
            }
            e.IdUser = User.Identity.GetId();
            e.Type = (int)EventTypeEnum.Event;
            return new ContentResult() { Content = JsonHelper.GetJsonString(BLLConflicts.AddEvent(e)), ContentType = "application/json" };
        }

        public ActionResult PreSentence(int conflictId)
        {
            var conflict = BLLConflicts.GetConflictForArbitration(conflictId);
            var demandeur = conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser);
            if (demandeur.IsLawyer != null && demandeur.IsLawyer.Value)
            {
                demandeur = conflict.UsersInConflicts.First(c => c.IdLawyer == conflict.IdCreationUser);
            }
            var defendeur = conflict.UsersInConflicts.First(c => (c.IsLawyer == null || !c.IsLawyer.Value) && c.IdUser != demandeur.IdUser);
            if(demandeur.UserCompany == null || demandeur.UserCompany.Company == null ||
                defendeur.UserCompany == null || defendeur.UserCompany.Company == null)
            {
                TempData["Error"] = "Vous devez renseigner les informations d'entreprises de chaque parties avant de pouvoir générer la pré-sentence";
                return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
            }

            var file = DocGenerator.GeneratePreSentenceReport(conflict);
            var azureFile = AzureFileHelper.AddFile(conflictId, file.FileStream, file.FileName);
            return View("WritePreSentence", new WritePreSentenceModel() { Conflict = conflict, Url = azureFile.Uri.AbsoluteUri });
        }

        public ActionResult Sentence(int conflictId)
        {
            var conflict = BLLConflicts.GetConflictForArbitration(conflictId);
            var file = DocGenerator.GenerateSentenceReport(conflict);
            var azureFile = AzureFileHelper.AddFile(conflictId, file.FileStream, file.FileName);
            return View("WriteSentence", new WritePreSentenceModel() { Conflict = conflict, Url = azureFile.Uri.AbsoluteUri });
        }

        public ActionResult WritePreSentence(int conflictId, HttpPostedFileBase FileClause)
        {
            BLLConflicts.UpdateConflictState(conflictId, (int)ConflictState.PreConcluded);

            var evt = BLLConflicts.AddEvent(new Event()
            {
                DateBegin = DateTime.Now,
                Description = "L'arbitre " + User.Identity.FirstName() + " " + User.Identity.LastName() + " a rédigé sa pré-sentence. Vous la retrouverez ci-dessous.",
                IdConflict = conflictId,
                Name = "Pré-sentence",
                Type = (int)EventTypeEnum.OfficialDocuments,
                IdUser = User.Identity.GetId()
            });

            var azureFile = AzureFileHelper.AddFile(conflictId, FileClause);
            var prooFile = BLLConflicts.AddFile(new ProofFile()
            {
                Description = "Pré-sentence",
                FilePath = azureFile.Uri.AbsoluteUri,
                FileType = (int)FileTypeEnum.PDF,
                IdConflict = conflictId,
                Name = "Pré-sentence",
                UploadDate = DateTime.Now,
            }, evt.Id);

            var csh = new ConflictStateHistoric()
            {
                IdConflict = conflictId,
                ConflictStateId = (int)ConflictState.PreConcluded,
                ConflictStateName = MetadataHelper.GetEnumDisplayNAme(ConflictState.PreConcluded),

            };
         
            BLLConflicts.AddConflictStateHistoric(csh);

            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        public ActionResult WriteSentence(int conflictId, HttpPostedFileBase FileClause)
        {
            BLLConflicts.UpdateConflictState(conflictId, (int)ConflictState.Concluded);

            var evt = BLLConflicts.AddEvent(new Event()
            {
                DateBegin = DateTime.Now,
                Description = "L'arbitre " + User.Identity.FirstName() + " " + User.Identity.LastName() + " a rédigé sa sentence. Vous la retrouverez ci-dessous.",
                IdConflict = conflictId,
                Name = "Sentence",
                Type = (int)EventTypeEnum.OfficialDocuments,
                IdUser = User.Identity.GetId()
            });

            var azureFile = AzureFileHelper.AddFile(conflictId, FileClause);
            var prooFile = BLLConflicts.AddFile(new ProofFile()
            {
                Description = "Sentence",
                FilePath = azureFile.Uri.AbsoluteUri,
                FileType = (int)FileTypeEnum.PDF,
                IdConflict = conflictId,
                Name = "Sentence",
                UploadDate = DateTime.Now,
            }, evt.Id);

            var csh = new ConflictStateHistoric()
            {
                IdConflict = conflictId,
                ConflictStateId = (int)ConflictState.Concluded,
                ConflictStateName = MetadataHelper.GetEnumDisplayNAme(ConflictState.Concluded),

            };

            BLLConflicts.AddConflictStateHistoric(csh);

            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        public ActionResult WriteProcedureClosure(int conflictId, int debateId, HttpPostedFileBase FileClause)
        {
            var debat = BLLDebates.GetDebate(debateId);
            var evt = BLLConflicts.AddEvent(new Event()
            {
                DateBegin = DateTime.Now,
                Description = "Ordonnance pour la procédure d'incidence \""+ debat.Title +"\" ",
                IdConflict = conflictId,
                Name = "Ordonnance de procédure d'incidence",
                Type = (int)EventTypeEnum.OfficialDocuments,
                IdUser = User.Identity.GetId()
            });

            var azureFile = AzureFileHelper.AddFile(conflictId, FileClause);
            var prooFile = BLLConflicts.AddFile(new ProofFile()
            {
                Description = "Sentence",
                FilePath = azureFile.Uri.AbsoluteUri,
                FileType = (int)FileTypeEnum.PDF,
                IdConflict = conflictId,
                Name = "Ordonnance_Procedure_Incidence_N°_" + debateId,
                UploadDate = DateTime.Now,
            }, evt.Id);

            BLLLegalDocuments.AddLegalDocument(new LegalDocument() { IdConflict = conflictId, Filename = "Ordonnance_Procedure_Incidence_N°_" + debateId, Url = azureFile.Uri.AbsoluteUri, Type = (int)LegalDocumentTypeEnum.OrdonnanceProcedure });

            BLLDebates.CloseDebate(debateId);

            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }


    }

}
