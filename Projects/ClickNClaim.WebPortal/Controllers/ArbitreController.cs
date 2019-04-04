using ClickNClaim.Business;
using ClickNClaim.Common;
using ClickNClaim.WebPortal.Models;
using ClickNClaim.WebPortal.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClickNClaim.WebPortal.Extensions;
using ClickNClaim.Common.Enums;
using ClickNClaim.WebPortal.Helpers;

namespace ClickNClaim.WebPortal.Controllers
{
    [Authorize]
    public class ArbitreController : Controller
    {
        // GET: Arbiter
        public ActionResult Centre()
        {
            var model = new CentreModel();
            model.ConflictsToAssigned = BLLConflicts.SearchConflicts(Common.ConflictState.ArbitrationReady);
            model.ConflictsAssigned = BLLConflicts.SearchConflicts(Common.ConflictState.ArbiterAssigned);
            model.ArbitrationStarted = BLLConflicts.SearchConflicts(Common.ConflictState.ArbitrationStarted, true);
            return View(model);
        }

        public ActionResult Refuse(int conflictId, string text)
        {
            var conflict = BLLConflicts.GetFullConflict(conflictId);
            conflict.State = (int)ConflictState.RefusedByPlateform;
            BLLConflicts.UpdateConflict(conflict);

            foreach (var item in conflict.UsersInConflicts)
            {
                Guid guid = Guid.NewGuid();

                FastArbitreEmails.CaseRefused(item.User.Email, text, Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
            }
            return RedirectToAction("Centre");

        }

        public ActionResult Assign(int conflictId, string arbiterId)
        {
            var conflict = BLLConflicts.GetConflictForArbitration(conflictId);
            var demandeur = conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser);
            if (demandeur.IsLawyer != null && demandeur.IsLawyer.Value)
            {
                demandeur = conflict.UsersInConflicts.First(c => c.IdLawyer == conflict.IdCreationUser);
            }
            var defendeur = conflict.UsersInConflicts.First(c => (c.IsLawyer == null || !c.IsLawyer.Value) && c.IdUser != demandeur.IdUser);

            if(demandeur.UserCompany == null || demandeur.UserCompany.Company == null || !demandeur.UserCompany.Company.isFilled() ||
                defendeur.UserCompany == null || defendeur.UserCompany.Company == null || !defendeur.UserCompany.Company.isFilled())
            {
                TempData["Error"] = "Vous devez avoir rempli les formulaires administratif des parties (adresses,rcs,etc) avant de pouvoir assigner le cas.";
                return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
            }



            BLLArbiter.AssignArbiterToConflict(conflictId, arbiterId);
            var arbiter = BLLUsers.GetUserById(arbiterId);


            //Generate Mission Order and save it on cloud
            var stream = DocGenerator.GenerateMissionOrder(conflictId);
            var blob = AzureFileHelper.AddFile(conflictId, stream.FileStream, stream.FileName);
            stream.FileStream.Dispose();
            BLLLegalDocuments.AddLegalDocument(new LegalDocument() { IdConflict = conflictId, Filename = stream.FileName, Url = blob.Uri.AbsoluteUri, Type = (int) LegalDocumentTypeEnum.MissionAct });
            Guid guid = Guid.NewGuid();
            FastArbitreEmails.NewMissionOrder(arbiter.Email, blob.Uri.AbsoluteUri, Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

            return RedirectToAction("Centre");
        }

        [AllowAnonymous]
        public ActionResult ArbiterList()
        {
            return View(BLLUsers.ListArbiters());
        }

        public ContentResult GetArbiterList()
        {
            return new ContentResult() { Content = JsonHelper.GetJsonString(BLLUsers.ListArbiters()), ContentType = "application/json" };
        }

        public ActionResult ProcedureArbitreJoin()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("Arbitre/{id}/{name}")]
        public ActionResult Detail(string id, string name)
        {
            if (User.Identity.GetId() == id)
            {
                return View(BLLUsers.GetFullArbiter(id));
            }
            return View(BLLUsers.GetArbiter(id));
        }

        public ContentResult GenerateAcceptanceReport(int conflictId)
        {
            var file = DocGenerator.GenerateAcceptanceDeclaration(BLLConflicts.GetConflict(conflictId), User.Identity.FirstName() + " " + User.Identity.LastName());
            var azureFile = AzureFileHelper.AddFile(conflictId, file.FileStream, file.FileName);
            return new ContentResult() { Content = azureFile.Uri.AbsoluteUri };
        }

        public ActionResult ValidateAcceptance(int conflictId, bool noconflict, bool legitimate, HttpPostedFileBase FileClause)
        {
            BLLArbiter.AcceptConflict(conflictId, User.Identity.GetId(), noconflict, legitimate);

            var legalDoc = BLLLegalDocuments.GetLastDocumentForType(conflictId, (int)LegalDocumentTypeEnum.MissionAct);

            var et = BLLConflicts.AddEvent(new Event()
            {
                DateBegin = legalDoc != null && legalDoc.CreateDate != null ? legalDoc.CreateDate.Value : DateTime.Now,
                Description = "Acte de mission désignant l arbitre",
                IdConflict = conflictId,
                Name = "Acte de mission",
                Type = (int)EventTypeEnum.OfficialDocuments,
                IdUser = User.Identity.GetId()
            });
          
            BLLConflicts.AddFile(new ProofFile()
            {
                Description = "Acte de mission désignant l arbitre",
                FilePath = legalDoc.Url,
                FileType = (int)FileTypeEnum.PDF,
                IdConflict = conflictId,
                Name = "Acte de mission",
                UploadDate = DateTime.Now,
            }, et.Id);

            var evt = BLLConflicts.AddEvent(new Event()
            {
                DateBegin = DateTime.Now,
                Description = "L'arbitre " + User.Identity.FirstName() + " " + User.Identity.LastName() + " a accepté de prendre en charge votre dossier. Il a par ailleurs déclaré n'avoir aucun conflit d'intérêt dans votre dossier et être en mesure de le prendre en charge.",
                IdConflict = conflictId,
                Name = "Déclaration d'acceptation et d'indépendance de l'arbitre",
                Type = (int)EventTypeEnum.OfficialDocuments,
                IdUser = User.Identity.GetId()
            });

            var azureFile = AzureFileHelper.AddFile(conflictId, FileClause);
            BLLConflicts.AddFile(new ProofFile()
            {
                Description = "Déclaration d'acceptation et d'indépendance de l'arbitre",
                FilePath = azureFile.Uri.AbsoluteUri,
                FileType = (int)FileTypeEnum.PDF,
                IdConflict = conflictId,
                Name = "Déclaration d'acceptation et d'indépendance de l'arbitre",
                UploadDate = DateTime.Now,
            }, evt.Id);



            var csh = new ConflictStateHistoric()
            {
                IdConflict = conflictId,
                ConflictStateId = (int)ConflictState.ArbitrationStarted,
                ConflictStateName = MetadataHelper.GetEnumDisplayNAme((ConflictState)ConflictState.ArbitrationStarted),
            };
            BLLConflicts.AddConflictStateHistoric(csh);
            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        public ActionResult AcceptCase(int conflictId)
        {
            return View("Acceptance", BLLConflicts.GetConflict(conflictId));
        }

        public ActionResult RefuseAssignation(int conflictId, string text, string whereFrom)
        {
            var conflict = BLLConflicts.GetConflict(conflictId);
            conflict.State = (int)ConflictState.ArbitrationReady;
            BLLConflicts.UpdateConflict(conflict);
            Guid guid = Guid.NewGuid();
            FastArbitreEmails.ArbitreRefuseAssignation(text, conflictId, User.Identity.FirstName() + " " + User.Identity.LastName(), Url.Action("Index", "Email", new { id = guid.ToString() }, this.Request.Url.Scheme), guid);
            if (whereFrom == "Centre")
            {
                return RedirectToAction("Centre");
            }
            else
                return RedirectToAction("Detail", new { id = User.Identity.GetId() });
        }

        [HttpPost]
        public ActionResult SaveNewSkills(string[] skills)
        {
            var userId = User.Identity.GetId();
            BLLArbiter.AddSkills(skills, userId);
            return RedirectToAction("Detail", new { id = userId, name = User.Identity.FirstName() + " " + User.Identity.LastName() });
        }


        public ActionResult StartDebate(int conflictId, int eventId, string debatType, string mailTitle, string mailBody, string visioTitle, string visioDescription, DateTime? rdv, int? debatTime)
        {
            var debate = new Debate();
            debate.CreateDate = DateTime.Now;
            debate.IdEvent = eventId;
            debate.CountDown = debatTime;
            debate.Type = debatType == "mail" ? (int)DebateTypeEnum.Mail : (int)DebateTypeEnum.Visio;
            debate.Title = debate.Type == (int)DebateTypeEnum.Mail ? mailTitle : visioTitle;
            debate = BLLDebates.CreateDebate(debate);

            if (debate.Type == (int)DebateTypeEnum.Mail)
            {
                var mailDebate = new MailDebate();
                mailDebate.CreateDate = DateTime.Now;
                mailDebate.CreatedBy = User.Identity.GetId();
                mailDebate.IdDebate = debate.Id;
                mailDebate.Body = mailBody;
                mailDebate = BLLDebates.AddMailDebate(mailDebate);

                if (debate.MailDebates == null)
                {
                    debate.MailDebates = new List<MailDebate>();
                }
                foreach (var item in BLLDebates.GetUsersForDebate(debate.Id))
                {
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.YouHaveMail(item, debate, Url.Action("MailDebate", "Arbitre", new { debatId = debate.Id }, this.Request.Url.Scheme) + "#mail_" + mailDebate.Id, Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                }

                debate.MailDebates.Add(mailDebate);
                return RedirectToAction("MailDebate", new { debatId = debate.Id });
            }
            else if (debate.Type == (int)DebateTypeEnum.Visio)
            {
                var conflict = BLLConflicts.GetConflictWithUsers(conflictId);
                var chatRoom = new ClickNClaim.OpenFireConnector.chatRoom(debate.Title, visioDescription, rdv.Value);
                OpenFireConnector.Connector connector = new OpenFireConnector.Connector("http://openfire-444e60hk.cloudapp.net:9090/", "admin", "SF211084agantio");
                foreach (var item in conflict.UsersInConflicts)
                {
                    var visioDebate = new MeetingDebate();
                    visioDebate.CreateDate = DateTime.Now;
                    visioDebate.Date = rdv.Value;
                    visioDebate.Description = visioDescription;

                    // visioDebate.IdUser = item.IdUser;
                    visioDebate.Link = "https://openfire-444e60hk.cloudapp.net:7443/ofmeet/?b=1-1461653024012";
                    visioDebate.Title = debate.Title;
                    //visioDebate.IdDebate = debate.Id;
                    //visioDebate = BLLDebates.AddMeetingDate(visioDebate);
                    //if (debate.MeetingDebates == null)
                    //{
                    //    debate.MeetingDebates = new List<MeetingDebate>();
                    //}
                    //debate.MeetingDebates.Add(visioDebate);
                    var user = BLLUsers.GetUserById(item.IdUser);
                    var existingUserInOpenFire = connector.UserExists(user.DisplayName.Replace(" ", "."));
                    if (existingUserInOpenFire == null)
                    {
                        existingUserInOpenFire = connector.CreateUser(new OpenFireConnector.user() { email = user.Email, name = user.DisplayName.Replace(' ', '.'), password = user.Email, username = user.DisplayName.Replace(' ', '.') });
                    }

                    chatRoom.members.Add(new OpenFireConnector.member() { value = user.DisplayName.Replace(" ", ".").ToLower() + "@openfire" });
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.VisioConferencePlanned(user.Email, rdv, visioTitle, visioDescription, connector.RoomLink + chatRoom.naturalName,
                        user.DisplayName.Replace(" ", ".").ToLower(), user.Email,
                         Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

                }
                if (chatRoom.members.Where(c => c.value == conflict.Arbiter.DisplayName.Replace(" ", ".") + "@openfire").FirstOrDefault() != null)
                {
                    chatRoom.members.RemoveAll(c => c.value == conflict.Arbiter.DisplayName.Replace(" ", ".") + "@openfire");
                }
                chatRoom.owners.Add(new OpenFireConnector.owner() { value = conflict.Arbiter.DisplayName.Replace(" ", ".") + "@openfire" });

                var link = connector.CreateChatroom(chatRoom);

                connector = null;
            }
            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        public ActionResult MailDebateAdd(int debatId, string mailBody)
        {
            var debate = BLLDebates.GetDebate(debatId);
            if (String.IsNullOrWhiteSpace(mailBody) && Request.Files["FileUpload"] != null && Request.Files["FileUpload"].ContentLength > 0)
            {
                HttpPostedFileBase hpf = Request.Files["FileUpload"] as HttpPostedFileBase;

                var blockBlob = AzureFileHelper.AddFile(debate.Event.IdConflict, hpf);

                var f = new ProofFile();
                f.FilePath = blockBlob.Uri.AbsoluteUri;
                f.IdConflict = debate.Event.IdConflict;
                f.Name = blockBlob.Name;

                var file = BLLConflicts.AddFile(f);
                var mailDebate = BLLDebates.AddMailDebate(new MailDebate()
                {
                    IdDebate = debatId,
                    Body = "<a href='" + file.FilePath + "'>" + file.Name + "</a>",
                    CreateDate = DateTime.Now,
                    CreatedBy = User.Identity.GetId(),
                    IdFile = file.Id
                });

                foreach (var item in BLLDebates.GetUsersForDebate(debatId))
                {
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.YouHaveMail(item, debate, Url.Action("MailDebate", "Arbitre", new
                    {
                        debatId = debate.Id
                    },
                    this.Request.Url.Scheme) + "#mail_" + mailDebate.Id,
                     Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                }
            }
            else
            {
                var mailDebate = BLLDebates.AddMailDebate(new MailDebate() { IdDebate = debatId, Body = mailBody, CreateDate = DateTime.Now, CreatedBy = User.Identity.GetId() });

                foreach (var item in BLLDebates.GetUsersForDebate(debatId))
                {
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.YouHaveMail(item, debate,
                        Url.Action("MailDebate", "Arbitre", new { debatId = debate.Id }, this.Request.Url.Scheme) + "#mail_" + mailDebate.Id,
                         Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                }
            }
            return RedirectToAction("MailDebate", new { debatId = debatId });
        }

        public ActionResult VisioDebate(int debatId)
        {
            return View(BLLDebates.GetDebate(debatId));
        }

        public ActionResult MailDebate(int debatId)
        {
            var debat = BLLDebates.GetDebate(debatId);
            if (debat.CountDown != null)
            {
                if (debat.CreateDate.Value.AddDays(debat.CountDown.Value) < DateTime.Now)
                {
                    BLLDebates.CloseDebate(debatId);
                }
            }
            return View(debat);
        }
        public ActionResult ShowDebate(int debatId)
        {
            var debat = BLLDebates.GetDebate(debatId);
            if (debat.Type == (int)DebateTypeEnum.Mail)
            {
                return RedirectToAction("MailDebate", new { debatId = debatId });
            }
            else
            {
                return RedirectToAction("VisioDebate", new { debatId = debatId });
            }
        }

        public ActionResult DatePropositions(int conflictId, List<DateTime> rdv, string visioTitle, string visioDescription, string estimateDuration)
        {
            var conflict = BLLConflicts.GetConflictWithUsers(conflictId);
            var evt = BLLConflicts.AddEvent(new Event()
            {
                IdUser = User.Identity.GetId(),
                DateBegin = DateTime.Parse("01/01/0001"),
                Name = visioTitle,
                Description = visioDescription,
                Type = (int)EventTypeEnum.Visio,
                IdConflict = conflictId
            });

            var visioDebate = new MeetingDebate();
            visioDebate.CreateDate = DateTime.Now;
            visioDebate.Description = visioDescription;
            visioDebate.IdConflict = conflictId;
            visioDebate.EstimateDuration = estimateDuration;
            visioDebate.Link = "http://openfire-444e60hk.cloudapp.net:7443/ofmeet/?b=1-1461653024012";
            visioDebate.Title = visioTitle;
            visioDebate.IdEvent = evt.Id;
            visioDebate = BLLDebates.AddMeetingDate(visioDebate);
            BLLMeetingDoodle.AddMeetingDebatePropositions(visioDebate.Id, rdv.Select(c => new MeetingProposition() { IdMeetingDebate = visioDebate.Id, DateTimeProposition = c }).ToList());
            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        public ActionResult Closure(int id, int conflictId)
        {
            var debate = BLLDebates.CloseDebate(id);
            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        public ActionResult ProcedureClosure(int id, int conflictId)
        {
            var conflict = BLLConflicts.GetConflictForArbitration(conflictId);
            var debate = BLLDebates.GetDebate(id);
            var file = DocGenerator.GenerateOrdonnanceProcedure(conflict, debate);
            var azureFile = AzureFileHelper.AddFile(conflictId, file.FileStream, file.FileName);
            return View("ProcedureClosure", new ProcedureClosureModel() { Conflict = conflict, Debat = debate, DocumentUrl = azureFile.Uri.AbsoluteUri });
        }

        public ActionResult ReOpen(int id, int conflictId, int countDown)
        {
            var debate = BLLDebates.ReOpenDebate(id, countDown);
            return RedirectToAction("MailDebate", new { debatId = id });
        }

        public ContentResult UpdateArbiterPresentation(ArbiterInformation arbiterInformation)
        {
            try
            {
                BLLArbiter.UpdatePresentation(arbiterInformation);
                return new ContentResult() { Content = "OK" };
            }
            catch (Exception e)
            {
                return new ContentResult() { Content = "KO" };
            }
        }

        public ActionResult ContactArbitre(int conflictId, string text)
        {

            Message m = new Message();
            m.CreateDate = DateTime.Now;
            m.IdConflict = conflictId;
            m.IdCreatedBy = User.Identity.GetId();
            m.Text = text;
            m = BLLConflicts.AddMessage(m);

            var conflict = BLLConflicts.GetConflictForArbitration(conflictId);
            if (User.Identity.Email() == conflict.Arbiter.Email)
            {
                foreach (var item in conflict.UsersInConflicts)
                {
                    if (!item.IsRepresented)
                    {
                        Guid guid = Guid.NewGuid();
                        FastArbitreEmails.SendMessageToArbiter(item.User.Email, conflict.Arbiter.DisplayName, conflictId, Url.Action("Messages", "Arbitre", new { conflictId = conflictId }, this.Request.Url.Scheme), Url.Action("Index", "Email", new { id = guid.ToString() }, this.Request.Url.Scheme), guid);
                    }
                }
            }
            else
            {
                foreach (var item in conflict.UsersInConflicts)
                {
                    if (!item.IsRepresented && item.IdUser != User.Identity.GetId())
                    {
                        Guid guid = Guid.NewGuid();
                        FastArbitreEmails.SendMessageToArbiter(item.User.Email, User.Identity.FirstName() + " " + User.Identity.LastName(), conflictId, Url.Action("Messages", "Arbitre", new { conflictId = conflictId }, this.Request.Url.Scheme), Url.Action("Index", "Email", new { id = guid.ToString() }, this.Request.Url.Scheme), guid);
                    }
                }
                Guid gud = Guid.NewGuid();
                FastArbitreEmails.SendMessageToArbiter(conflict.Arbiter.Email, User.Identity.FirstName() + " " + User.Identity.LastName(), conflictId, Url.Action("Messages", "Arbitre", new { conflictId = conflictId }, this.Request.Url.Scheme), Url.Action("Index", "Email", new { id = gud.ToString() }, this.Request.Url.Scheme), gud);
            }


            TempData["Success"] = "Votre message a bien été envoyé!";
            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        public ActionResult ContactParty(int conflictId, string text, string partyName)
        {
            Message m = new Message();
            m.CreateDate = DateTime.Now;
            m.IdConflict = conflictId;
            m.IdCreatedBy = User.Identity.GetId();
            m.Text = text;
            m = BLLConflicts.AddMessage(m);

            var conflict = BLLConflicts.GetConflictForArbitration(conflictId);
            foreach (var item in conflict.UsersInConflicts)
            {
                if (!item.IsRepresented)
                {
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.SendMessageToParty(item.User.Email, partyName, conflictId, Url.Action("Messages", "Arbitre", new { conflictId = conflictId }, this.Request.Url.Scheme), Url.Action("Index", "Email", new { id = guid.ToString() }, this.Request.Url.Scheme), guid);
                }
            }
            TempData["Success"] = "Votre message a bien été envoyé!";
            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });

        }

        public ActionResult Messages(int conflictId)
        {
            return View("Messages", BLLConflicts.GetConflictMessages(conflictId));
        }

        public ActionResult StartProcedure(int conflictId, int debatTime, string mailBody, string mailTitle)
        {
            var evt = BLLConflicts.AddEvent(new Event()
            {
                DateBegin = DateTime.Now,
                Description = mailBody,
                IdConflict = conflictId,
                Name = mailTitle,
                Type = (int)EventTypeEnum.OfficialDocuments,
                IdUser = User.Identity.GetId()
            });
            var debate = new Debate();
            debate.CreateDate = DateTime.Now;
            debate.IdEvent = evt.Id;
            debate.CountDown = debatTime;
            debate.Type = (int)DebateTypeEnum.Mail;
            debate.Title = mailTitle;
            debate = BLLDebates.CreateDebate(debate);

            var mailDebate = new MailDebate();
            mailDebate.CreateDate = DateTime.Now;
            mailDebate.CreatedBy = User.Identity.GetId();
            mailDebate.IdDebate = debate.Id;
            mailDebate.Body = mailBody;
            mailDebate = BLLDebates.AddMailDebate(mailDebate);

            if (debate.MailDebates == null)
            {
                debate.MailDebates = new List<MailDebate>();
            }
            foreach (var item in BLLDebates.GetUsersForDebate(debate.Id))
            {
                Guid guid = Guid.NewGuid();
                FastArbitreEmails.YouHaveMail(item, debate, Url.Action("MailDebate", "Arbitre", new { debatId = debate.Id }, this.Request.Url.Scheme) + "#mail_" + mailDebate.Id, Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
            }

            debate.MailDebates.Add(mailDebate);
            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });

        }

    }
}