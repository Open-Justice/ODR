using ClickNClaim.Business;
using ClickNClaim.Common;
using ClickNClaim.WebPortal.Models;
using ClickNClaim.WebPortal.Properties;
using ClickNClaim.WebPortal.Tools;
using ClickNClaim.WebPortal.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClickNClaim.Common.Enums;
using ClickNClaim.WebPortal.Helpers;
using ClickNClaim.Common.Attributes;

namespace ClickNClaim.WebPortal.Controllers
{
    //[RequireHttps]
    [Authorize]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
    [ConflictNarrower]
    public partial class ConflictController : Controller
    {
        [AllowAnonymous]
        public async Task<ActionResult> Create(string firstname, string lastname, string email)
        {

            var u = BLLUsers.GetUserByEmail(email);
            if (u != null)
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account", new { ReturnUrl = Url.Action("Create", "Conflict", new { firstname = firstname, lastname = lastname, email = email }) });
                }
                var conflict = BLLConflicts.AddConflict(new Conflict() { IdCreationUser = u.Id });
                return RedirectToAction("Identification", new { conflictId = conflict.Id });
            }
            else
            {
                if (User.Identity.IsAuthenticated)
                {
                    var conflict = BLLConflicts.AddConflict(new Conflict() { IdCreationUser = User.Identity.GetUserId() });
                    return RedirectToAction("Identification", new { conflictId = conflict.Id });
                }

                //if (String.IsNullOrWhiteSpace(email))
                //{
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Create", "Conflict") });
               // }

                //var user = new ApplicationUser { UserName = email, Email = email, CreateDate = DateTime.Now, FirstName = firstname, LastName = lastname };
                //var pwd = Guid.NewGuid().ToString().Substring(0, 6);
                //var result = await UserManager.CreateAsync(user, pwd);
                //if (result.Succeeded)
                //{
                //    Session["oi3_C309"] = email;
                //    Guid guid = Guid.NewGuid();
                //    FastArbitreEmails.AccountCreated(email, firstname, pwd, Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

                //    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                //    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                //    return RedirectToAction("ShouldConfirmEmail", "Account");

                //}
                //else
                //{
                //    return Redirect("/");
                //}
            }
        }

        public ActionResult Delete(int conflictId)
        {
            BLLConflicts.DisableUserInConflict(conflictId, User.Identity.GetUserId());
            return RedirectToAction("Profil", "Account");
        }


        #region IDENTIFICATION MANAGEMENT

        [Route("Conflict/{conflictId}/Identification")]
        [AllowAnonymous]
        public ActionResult Identification(int conflictId, string idUser)
        {

            if (idUser == "userId")
            {
                if (User.Identity.IsAuthenticated)
                {
                    idUser = User.Identity.GetId();
                }
            }
            if (idUser == null && User.Identity.IsAuthenticated)
            {
                idUser = User.Identity.GetId();
            }

            var user = BLLUsers.GetUserById(idUser);

            if( user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Url });
            }


            var conflict = BLLConflicts.GetConflictForUser(conflictId, idUser);

            var uc = conflict.UsersInConflicts.Where(c => c.IdUser == idUser).FirstOrDefault();
            if (uc != null && uc.IsRepresented)
                return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });

            if (!String.IsNullOrWhiteSpace(Request.QueryString.Get("i")) && !conflict.UsersInConflicts.Any(c => c.IdUser == idUser))
            {
                var uic = BLLConflicts.AddUserInConflictFromInvitation(user.Email, Guid.Parse(Request.QueryString.Get("i")), user.Id);
                conflict.UsersInConflicts.Add(uic);
            }

            conflict = BLLConflicts.GetConflictForUser(conflictId, User.Identity.GetId());
            ViewBag.conflictId = conflictId;
            var model = new IdentificationModel();
            model.Conflict = conflict;
            if (model.Conflict.UsersInConflicts.ElementAt(0).IsRepresented)
            {
                model.Lawyer = BLLUsers.GetUserById(model.Conflict.UsersInConflicts.ElementAt(0).IdLawyer);
            }
            else
                model.Lawyer = new AspNetUser();
            if ((model.Conflict.UsersInConflicts.ElementAt(0).IsLawyer != null && model.Conflict.UsersInConflicts.ElementAt(0).IsLawyer.Value) ||
                model.Conflict.UsersInConflicts.ElementAt(0).IsRepresented)
            {
                if (model.Conflict.UsersInConflicts.ElementAt(0).IsLawyer != null && model.Conflict.UsersInConflicts.ElementAt(0).IsLawyer.Value)
                {
                    var uic = model.Conflict.UsersInConflicts.Where(c => c.IsRepresented &&
                                                                         c.IdLawyer == model.Conflict.UsersInConflicts.ElementAt(0).IdUser).FirstOrDefault();
                    if (uic != null)
                    {
                        model.UserRepresented = BLLUsers.GetUserById(uic.IdUser, model.Conflict.Id);
                    }
                }
                else
                {
                    model.UserRepresented = BLLUsers.GetUserById(model.Conflict.UsersInConflicts.ElementAt(0).IdUser, model.Conflict.Id);
                }
            }
            else
                model.UserRepresented = new AspNetUser();

            foreach (var item in conflict.Invitations.Where(c => !c.IsUsed).OrderBy(c => c.CreationDate))
            {
                model.Conflict.UsersInConflicts.Add(new UsersInConflict()
                {
                    IdConflict = model.Conflict.Id,
                    IdUser = item.Id.ToString(),
                    IsPhysical = String.IsNullOrWhiteSpace(item.CompanyName),
                    CompanyName = item.CompanyName,
                    User = new AspNetUser() { FirstName = item.FirstName, LastName = item.LastName, Email = item.Email }
                });
            }

            if (model.UserRepresented.UsersInConflicts == null)
            {
                model.UserRepresented.UsersInConflicts = new List<UsersInConflict>();
                model.UserRepresented.UsersInConflicts.Add(new UsersInConflict());

            }
            else if (model.UserRepresented.UsersInConflicts.Count == 0)
            {
                model.UserRepresented.UsersInConflicts.Add(new UsersInConflict());
            }


            if (conflict.IdCreationUser == User.Identity.GetId() || User.Identity.GetId() == conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser).IdLawyer)
                return View("Identification", model);
            else
            {
                IdentificationForOpponentModel otherModel = new IdentificationForOpponentModel();
                otherModel.Conflict = conflict;
                otherModel.Lawyer = new AspNetUser();
                otherModel.Me = conflict.UsersInConflicts.Where(c => c.IdUser == User.Identity.GetId()).FirstOrDefault();
                return View("IdentificationOpponent", otherModel);
            }
        }

        public EmptyResult RemoveInvitation(int conflictId, Guid id)
        {
            BLLInvitations.DeleteInvitation(id);
            return new EmptyResult();
        }

        [Route("Conflict/{conflictId}/UpdateIdentification")]
        public async Task<ActionResult> UpdateIdentification(IdentificationModel model, int conflictId)
        {
            var c = BLLConflicts.GetConflictForUser(conflictId, User.Identity.GetId());

            for (int i = 1; i < model.Conflict.UsersInConflicts.Count; i++)
            {
                var uc = model.Conflict.UsersInConflicts.ElementAt(i);


                if (!String.IsNullOrWhiteSpace(uc.IdUser))
                {
                    var invi = BLLInvitations.GetInvitation(Guid.Parse(uc.IdUser));
                    invi.FirstName = uc.User.FirstName;
                    invi.CompanyName = uc.CompanyName;
                    invi.LastName = uc.User.LastName;
                    invi.Email = uc.User.Email;
                    BLLInvitations.UpdateInvitation(invi);
                }
                else
                {

                    BLLInvitations.AddInvitation(new Invitation()
                    {
                        CompanyName = uc.CompanyName,
                        CreationDate = DateTime.Now,
                        Email = uc.User.Email,
                        FirstName = uc.User.FirstName,
                        IdConflict = model.Conflict.Id,
                        IsLawyer = false,
                        IsUsed = false,
                        LastName = uc.User.LastName,
                        Id = Guid.NewGuid()
                    });
                }
            }


            //LAWYER IS DECLARING CONFLICT
            if (model.Conflict.UsersInConflicts.ElementAt(0).IsLawyer != null && model.Conflict.UsersInConflicts.ElementAt(0).IsLawyer.Value)
            {
                var lawyerId = User.Identity.GetId();

                ///SETS USER AS LAWYER FOR THIS CONFLICT
                var uic = BLLConflicts.GetUserInConflict(lawyerId, model.Conflict.Id);
                uic.IsLawyer = true;
                uic.ReadyForArbitration = true;
                BLLConflicts.UpdateUserInConflict(uic);

                ///LOOK IF USER EXISTS
                var existingUser = BLLUsers.GetUserByEmail(model.UserRepresented.Email);

                //CREATE USER IF NOT EXISTS
                if (existingUser == null)
                {
                    var password = Guid.NewGuid().ToString().Substring(0, 6);
                    bool hasCapitalized = false;
                    for (int i = 0; i < password.Length; i++)
                    {
                        if (char.IsLetter(password[i]))
                        {
                            password = password.Substring(0, i) + password[i].ToString().ToUpper().First() + password.Substring(i+1);
                            hasCapitalized = true;
                            break;
                        }
                    }
                    if (!hasCapitalized)
                    {
                        password += "Z";
                    }

                    //CREATE USER
                    var identityResult = await UserManager.CreateAsync(new ApplicationUser()
                    {
                        CreateDate = DateTime.Now,
                        FirstName = model.UserRepresented.FirstName ?? model.UserRepresented.UsersInConflicts.ElementAt(0).CompanyName,
                        LastName = model.UserRepresented.LastName ?? String.Empty,
                        Email = model.UserRepresented.Email,
                        EmailConfirmed = true,
                        UserName = String.Format("{0} {1}", model.UserRepresented.FirstName, model.UserRepresented.LastName)
                    }, password);
                    Guid guidd = Guid.NewGuid();
                    FastArbitreEmails.AccountCreatedForUserByLawyer(model.UserRepresented.Email, password, Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Home"), Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guidd.ToString() }), guidd);

                    existingUser = BLLUsers.GetUserByEmail(model.UserRepresented.Email);
                }

                ///ADD TO CONFLICT
                var uicc = BLLConflicts.AddUserDirectly(existingUser.Id, model.Conflict.Id);
                uicc.IsPhysical = model.UserRepresented.UsersInConflicts.ElementAt(0).IsPhysical;
                uicc.CompanyName = model.UserRepresented.UsersInConflicts.ElementAt(0).CompanyName;
                uicc.UserFunction = model.UserRepresented.UsersInConflicts.ElementAt(0).UserFunction;
                uicc.IsRepresented = true;
                uicc.IdLawyer = lawyerId;
                BLLConflicts.UpdateUserInConflict(uicc);
                Guid guid = Guid.NewGuid();
                ///MAIL HIM
                FastArbitreEmails.LawyerStartedCase(existingUser.Email, BLLUsers.GetUserById(lawyerId).DisplayName, model.Conflict.Id,
                    Request.UrlReferrer.DnsSafeHost + Url.Action("Conflict", "Viewer", new { conflictId = model.Conflict.Id }),
                     Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

                BLLConflicts.UpdateUserInConflict(model.Conflict.UsersInConflicts.ElementAt(0));
            }
            ///CLIENT IS DECLARING CONFLICT
            else
            {
                ///CLIENT HAS LAWYER
                if (model.Conflict.UsersInConflicts.ElementAt(0).IsRepresented)
                {
                    var clientId = User.Identity.GetId();

                    //CHECK IF LAWYER EXISTS
                    var existingLawyer = BLLUsers.GetUserByEmail(model.Lawyer.Email);

                    if (existingLawyer == null)
                    {
                        var password = Guid.NewGuid().ToString().Substring(0, 6);
                        //CREATE USER
                        var identityResult = await UserManager.CreateAsync(new ApplicationUser()
                        {
                            CreateDate = DateTime.Now,
                            FirstName = model.Lawyer.FirstName,
                            LastName = model.Lawyer.LastName,
                            Email = model.Lawyer.Email,
                            EmailConfirmed = true,
                            UserName = model.Lawyer.Email
                        }, password);
                        existingLawyer = BLLUsers.GetUserByEmail(model.Lawyer.Email);
                        model.Conflict.UsersInConflicts.ElementAt(0).User = BLLUsers.GetUserById(model.Conflict.UsersInConflicts.ElementAt(0).IdUser);
                        Guid guid = Guid.NewGuid();
                        FastArbitreEmails.LawyerCalledOnCase(model.Lawyer.Email, existingLawyer.DisplayName, model.Conflict.Id,
                            model.Conflict.UsersInConflicts.ElementAt(0).User.DisplayName,
                            Request.UrlReferrer.DnsSafeHost + Url.HttpRouteUrl("Declaration", new { conflictId = model.Conflict.Id }),
                            model.Lawyer.Email, password,
                             Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

                    }

                    var lawyerUic = BLLConflicts.AddUserDirectly(existingLawyer.Id, model.Conflict.Id);
                    lawyerUic.IsLawyer = true;
                    BLLConflicts.UpdateUserInConflict(lawyerUic);

                    ///SETS USER AS REPRESENTED FOR THIS CONFLICT
                    var uic = BLLConflicts.GetUserInConflict(clientId, model.Conflict.Id);
                    uic.IsRepresented = true;
                    uic.IsLawyer = false;
                    uic.IdLawyer = existingLawyer.Id;
                    BLLConflicts.UpdateUserInConflict(uic);
                }
                else
                {
                    var uic = BLLConflicts.GetUserInConflict(model.Conflict.UsersInConflicts.ElementAt(0).IdUser, model.Conflict.Id);
                    uic.CompanyName = model.Conflict.UsersInConflicts.ElementAt(0).CompanyName;
                    uic.UserFunction = model.Conflict.UsersInConflicts.ElementAt(0).UserFunction;
                    BLLConflicts.UpdateUserInConflict(uic);
                }
            }


            BLLConflicts.UpdateConflict(model.Conflict);
            return RedirectToAction("Declaration", new { idConflict = model.Conflict.Id });
        }

        public async Task<ActionResult> UpdateIdentificationForOpponent(IdentificationForOpponentModel model, int conflictId)
        {
            if (model.Me.IsRepresented)
            {
                var clientId = User.Identity.GetId();

                //CHECK IF LAWYER EXISTS
                var existingLawyer = BLLUsers.GetUserByEmail(model.Lawyer.Email);

                if (existingLawyer == null)
                {
                    var password = Guid.NewGuid().ToString().Substring(0, 6);
                    //CREATE USER
                    var identityResult = await UserManager.CreateAsync(new ApplicationUser()
                    {
                        CreateDate = DateTime.Now,
                        FirstName = model.Lawyer.FirstName,
                        LastName = model.Lawyer.LastName,
                        Email = model.Lawyer.Email,
                        EmailConfirmed = true,
                        UserName = String.Format("{0} {1}", model.Lawyer.FirstName, model.Lawyer.LastName)
                    }, password);
                    existingLawyer = BLLUsers.GetUserByEmail(model.Lawyer.Email);
                    var me = BLLUsers.GetUserById(clientId);
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.LawyerCalledOnCase(model.Lawyer.Email, existingLawyer.DisplayName,model.Conflict.Id, me.DisplayName,
                       Request.UrlReferrer.DnsSafeHost + Url.Action("Conflict", "Viewer", new { conflictId = model.Conflict.Id }),
                        model.Lawyer.Email, password,
                         Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

                }

                var lawyerUic = BLLConflicts.AddUserDirectly(existingLawyer.Id, model.Conflict.Id);
                lawyerUic.IsLawyer = true;
                BLLConflicts.UpdateUserInConflict(lawyerUic);

                ///SETS USER AS REPRESENTED FOR THIS CONFLICT
                var uic = BLLConflicts.GetUserInConflict(clientId, model.Conflict.Id);
                uic.IsRepresented = true;
                uic.IsLawyer = false;
                uic.IdLawyer = existingLawyer.Id;
                uic.IsPhysical = model.Me.IsPhysical;
                uic.CompanyName = model.Me.CompanyName;
                BLLConflicts.UpdateUserInConflict(uic);
            }
            return RedirectToAction("Declaration", new { conflictId = model.Conflict.Id });
        }

        #endregion

        #region EVENT MANAGEMENT

        [Route("Conflict/{userId}/{conflictId}/Events")]
        public ActionResult Events(int conflictId)
        {
            var conflict = BLLConflicts.GetFullConflict(conflictId);
            ViewBag.conflictId = conflictId;
            return View("Events", conflict);
        }

       
        [ValidateInput(false)]
        public ContentResult SaveEvent(Event e, int conflictId)
        {
            e.IdUser = User.Identity.GetId();
            Event evt = null;
            if (e.Id > 0)
            {
                evt = BLLConflicts.UpdateEvent(e);
            }
            else
            {
                evt = BLLConflicts.AddEvent(e);
            }
            return new ContentResult() { Content = JsonHelper.GetJsonString(evt), ContentType = "application/json" };

        }


        public ContentResult AddNewEvent(Event e, int conflictId)
        {
            if (e.Id != 0)
            {
                return new ContentResult() { Content = JsonHelper.GetJsonString(BLLConflicts.UpdateEvent(e)), ContentType = "application/json" };
            }
            e.IdUser = User.Identity.GetId();
            e.Type = (int)EventTypeEnum.Event;
            return new ContentResult() { Content = JsonHelper.GetJsonString(BLLConflicts.AddEvent(e)), ContentType = "application/json" };
        }

        public ContentResult RemoveEvent(int id, int conflictId)
        {
            var evt = BLLConflicts.GetEvent(id);
            if (evt != null)
            {
                BLLConflicts.RemoveEvent(id);
                foreach (var item in evt.ProofFiles)
                {
                    UploadController.DeleteFile(evt.IdConflict, item.Name);
                }
            }


            return Content("OK");
        }

        public ContentResult RemoveFile(int id, int conflictId, string fileName)
        {
            BLLConflicts.RemoveFile(id);
            UploadController.DeleteFile(conflictId, fileName);
            return Content("OK");
        }

        public JsonResult AddNewComment(Comment c, int conflictId)
        {
            c.IdUser = User.Identity.GetUserId();
            c.UserEmail = User.Identity.Email();
            var comment = BLLConflicts.AddComment(c);
            comment.CreatedBy = new AspNetUser();
            comment.CreatedBy.FirstName = User.Identity.FirstName();
            comment.CreatedBy.LastName = User.Identity.LastName();
            return new JsonResult() { Data = comment, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #endregion

        #region DECLARATION MANAGEMENT
      
        [Route("Conflict/{conflictId}/Declaration", Name = "Declaration", Order = 1)]
        public ActionResult Declaration(int conflictId)
        {
            var conflict = BLLConflicts.GetConflict(conflictId);

            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }

            var uc = conflict.UsersInConflicts.Where(c => c.IdUser == User.Identity.GetId()).FirstOrDefault();
            if (uc != null && uc.IsRepresented)
                return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });


            if (conflict.IdCreationUser == User.Identity.GetId() || User.Identity.GetId() == conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser).IdLawyer)
            {
                DeclarationModel dm = new DeclarationModel();
                dm.Conflict = BLLConflicts.GetConflict(conflictId);
                dm.UserInConflict = BLLConflicts.GetUserInConflict(User.Identity.GetId(), conflictId);

                ViewBag.conflictId = conflictId;
                return View("Declaration", dm);
            }
            else
            {
                DeclarationModel dom = new DeclarationModel();
                dom.Conflict = conflict;
                dom.UserInConflict = conflict.UsersInConflicts.Where(c => c.IdUser == User.Identity.GetId()).FirstOrDefault();

                var creator = conflict.UsersInConflicts.Where(c => c.IdUser == conflict.IdCreationUser).FirstOrDefault();
                 if (creator != null && creator.IsLawyer != null && creator.IsLawyer.Value)
                {
                    dom.PreviousDeclarations = new List<UsersInConflict>() { creator };
                    var client = conflict.UsersInConflicts.Where(c => c.IdLawyer == creator.IdUser).FirstOrDefault();
                    if (client != null)
                    {
                        dom.PreviousDeclarations[0].CompanyName = client.CompanyName;
                    }
                }
                else
                {
                    dom.PreviousDeclarations = conflict.UsersInConflicts.Where(c => c.IdUser != User.Identity.GetId() && !String.IsNullOrWhiteSpace(c.UserDescriptionOfTheConflict)).ToList();
                }
                ViewBag.conflictId = conflictId;
                return View("DeclarationOpponent", dom);
            }
        }
        [HttpPost]
        public ActionResult SaveDeclaration(DeclarationModel model, int conflictId)
        {
            try
            {
                BLLConflicts.SaveDeclaration(model.Conflict, model.UserInConflict, User.Identity.GetId());
                if (model.Conflict.HasArbitralClause && Request.Files["FileClause"].ContentLength > 0)
                {
                    var blob = AzureFileHelper.AddFile(model.Conflict.Id, Request.Files["FileClause"]);

                    BLLLegalDocuments.AddLegalDocument(new LegalDocument() { IdConflict = model.Conflict.Id, Filename = "Clause compromissoire", Url = blob.Uri.AbsoluteUri, Type= (int) LegalDocumentTypeEnum.CompromissoryClause });


                }
                if (model.Conflict.HasArbitralClause)
                {
                    var conflict = BLLConflicts.GetFullConflict(model.Conflict.Id);

                    conflict.State = (int)ConflictState.Pending;
                    BLLConflicts.UpdateConflict(conflict);
                    foreach (var invit in conflict.Invitations)
                    {
                        Guid guid = Guid.NewGuid();
                        FastArbitreEmails.InvitationToJoinConflict(invit, conflict,
                        String.Format("{0}&i={1}", Request.Url.Host.ToLower() + Url.Action("Conflict", "Viewer", new { conflictId = conflict.Id }), invit.Id),
                        User.Identity.Name,
                         Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

                    }
                    return RedirectToAction("Conflict", "Viewer", new { conflictId = model.Conflict.Id });
                }
                else
                {
                    return RedirectToAction("ConflictType", new { conflictId = model.Conflict.Id });
                }

            }
            catch (Exception e)
            {
                MailSender.SendMessage("pierrelasvigne@hotmail.com", "BUG IN FastArbitre", e.ToString());
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult SaveOpponentDeclaration(int conflictId, string declaration)
        {
            var userId = User.Identity.GetId();
            var conflict = BLLConflicts.GetConflictForUser(conflictId, userId);

            var uic = conflict.UsersInConflicts.First(c => c.IdUser == userId);
            uic.UserDescriptionOfTheConflict = declaration;
            BLLConflicts.UpdateUserInConflict(uic);

            if (conflict.State < (int)ConflictState.ArbitrationReady)
            {
                return RedirectToAction("Timeline", new { conflictId = conflict.Id });
            }
            else
            {
                return RedirectToAction("Conflict", "Viewer", new { conflictId = conflict.Id });
            }
        }

        [ConflictNarrower]
        [Route("Conflict/{idUser}/{idConflict}/SimpleDeclaration")]
        public ActionResult SimpleDeclaration(int conflictId)
        {
            ViewBag.conflictId = conflictId;

            return View(BLLConflicts.GetConflictForUser(conflictId, User.Identity.GetId()));
        }

        public ActionResult SaveSimpleDeclaration(int conflictId, string declaration, bool hasArbitralClause, HttpPostedFileBase clauseFile)
        {
            var conflict = BLLConflicts.GetConflict(conflictId);
            var uic = BLLConflicts.GetUserInConflict(User.Identity.GetId(), conflictId);
            uic.UserDescriptionOfTheConflict = declaration;

            BLLConflicts.UpdateUserInConflict(uic);

            conflict.HasArbitralClause = hasArbitralClause;
            BLLConflicts.UpdateConflict(conflict);

            if (hasArbitralClause)
            {
                var blob = AzureFileHelper.AddFile(conflictId, clauseFile);

                BLLLegalDocuments.AddLegalDocument(new LegalDocument() { IdConflict = conflictId, Filename = "Clause Compromisoire", Url = blob.Uri.AbsoluteUri, Type=(int)LegalDocumentTypeEnum.CompromissoryClause });
            }


            if (User.Identity.GetId() == conflict.IdCreationUser)
            {
                return RedirectToAction("ConflictType", new { conflictId = conflictId });
            }
            else
            {
                return RedirectToAction("Timeline", new { conflictId = conflictId });
            }
        }


        #endregion

        #region TIMELINE

        [ConflictNarrower]
        [Route("Conflict/{conflictId}/Timeline")]
        public ActionResult Timeline(int conflictId)
        {
            ViewBag.conflictId = conflictId;
            TimelineModel tm = new TimelineModel();
            tm.Conflict = BLLConflicts.GetConflictEvents(conflictId);
            tm.DisagreementModel = new Disagreement();
            return View("Timeline", tm);
        }

      


        #endregion

        #region RESOLUTION
        [ConflictNarrower]
        [Route("Conflict/{conflictId}/Resolution")]
        public ActionResult Resolution(int conflictId)
        {

            ResolutionModel rm = new ResolutionModel();
            rm.Conflict = BLLConflicts.GetConflictForResolutionContestation(conflictId);
            if (rm.Conflict.IdConflictType == null || !rm.Conflict.IdConflictType.HasValue)
            {
                TempData["Error"] = "Avant de déclarer vos demandes, merci de nous renseigner le type de conflit que vous rencontrez.";

                return RedirectToAction("ConflictType", new { conflictId = conflictId });
            }


            rm.ResolutionTypes = BLLRefs.ListResolutionTypesForConflict(rm.Conflict.IdConflictType.Value);
            rm.MyResolutions = BLLConflicts.GetUserResolutions(conflictId, User.Identity.GetUserId());

            foreach (var item in rm.MyResolutions)
            {
                var resoType = rm.ResolutionTypes.Where(c => c.Id == item.IdResolutionType).FirstOrDefault();
                if (resoType != null)
                {
                    resoType.IsSelected = true;
                    resoType.ResolutionComment = item.ResolutionComment;
                    if (item.Event != null)
                        resoType.ProofFiles = item.Event.ProofFiles.ToList();

                }
            }



            var userId = User.Identity.GetUserId();
            ViewBag.conflictId = conflictId;
            return View(rm);
        }


        [HttpPost]
        public ActionResult SaveResolutions(int conflictId, List<Resolution> myResolutions/*, string[] emails*/)
        {
            if (myResolutions != null && myResolutions.Count > 0)
            {
                var conflict = BLLConflicts.GetFullConflict(conflictId);

                var url = Url.Action("Conflict", "Viewer", new { conflictId = conflict.Id });

                var events = BLLConflicts.AddOrUpdateResolutionEvent(conflictId, User.Identity.GetId(), myResolutions);
                // BLLConflicts.ClearResolutionEvent(evt.Id);

                foreach (var item in myResolutions)
                {
                    BLLConflicts.AddOrUpdateResolution(item, User.Identity.Name, conflictId, User.Identity.GetId(), events.First(c => c.Name == item.Name).Id);

                }
                return RedirectToAction("Clause", new { conflictId = conflict.Id });
            }
            return RedirectToAction("Resolution", new { conflictId = conflictId });
        }

        [Route("Conflict/{conflictId}/Merci")]
        public ActionResult Merci(int conflictId)
        {
            ViewBag.conflictId = conflictId;
            return View(new MerciModel() { Conflict = BLLConflicts.GetConflict(conflictId) });
        }

        public ActionResult Acceptance(Conflict conflict, int conflictId)
        {

            BLLConflicts.AcceptArbitration(conflict.Id, User.Identity.GetId());
            BLLConflicts.MarkUserDeclarationHasCompleted(conflict.Id, User.Identity.GetId());
            var previous = BLLConflicts.GetFullConflict(conflict.Id);



            if (conflict.HasArbitralClause)
            {
                previous.HasArbitralClause = true;
            }

            BLLConflicts.UpdateConflict(previous);


            if (conflict.HasArbitralClause && Request.Files["FileClause"].ContentLength > 0)
            {

                var blob = AzureFileHelper.AddFile(conflict.Id, Request.Files["FileClause"]);

                BLLLegalDocuments.AddLegalDocument(new LegalDocument() { IdConflict = conflict.Id, Filename = "Clause compromissoire", Url = blob.Uri.AbsoluteUri, Type = (int)LegalDocumentTypeEnum.CompromissoryClause });
            }

            UsersInConflict demandeur = null;
            UsersInConflict defendeur = null;
            var parties = new List<UsersInConflict>();
            var creator = previous.UsersInConflicts.Where(c => c.IdUser == previous.IdCreationUser).FirstOrDefault();
            if (creator.IsLawyer != null && creator.IsLawyer.Value)
            {
                demandeur = previous.UsersInConflicts.Where(c => c.IdLawyer == creator.IdUser).FirstOrDefault();
                demandeur.ReadyForArbitration = creator.ReadyForArbitration;
            }
            else
            {
                demandeur = creator;
            }
            if(demandeur != null)
            {
                parties.Add(demandeur);
            }

            var def = previous.UsersInConflicts.Where(c => c.IdUser != demandeur.IdUser && (c.IsLawyer == null || !c.IsLawyer.Value)).ToList();
            if(def.Count > 0)
            {
                defendeur = def.Where(c => !String.IsNullOrWhiteSpace(c.UserDescriptionOfTheConflict)).FirstOrDefault();
            }
            if( defendeur != null)
            {
                parties.Add(defendeur);
            }
           

            

            if (parties.Count > 1 && parties.All(c => c.ReadyForArbitration != null &&
                                                                                         c.ReadyForArbitration.HasValue &&
                                                                                         c.ReadyForArbitration.Value))
            {
                previous.State = (int)ConflictState.Open;
                BLLConflicts.UpdateConflict(previous);
            }

            if (previous.State == (int)ConflictState.Created)
            {
                return RedirectToAction("Merci", new { conflictId = conflict.Id });
            }
            Guid guid = Guid.NewGuid();

            FastArbitreEmails.ConfirmContestationFilledIn(User.Identity.Email(), string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~")) + Url.Action("Conflict", "Viewer", new { conflictId = conflict.Id }),
                string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~")) + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);

            foreach (var item in previous.UsersInConflicts)
            {
                guid = Guid.NewGuid();
                FastArbitreEmails.EndFreePhase(item.User.Email, item.User.DisplayName, string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~")) + Url.Action("Conflict", "Viewer", new { conflictId = conflict.Id }),
                string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~")) + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
            }

         

            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflict.Id });
        }

        [Route("Conflict/{conflictId}/Clause")]
        public ActionResult Clause(int conflictId)
        {
            ViewBag.conflictId = conflictId;
            ClauseModel cm = new ClauseModel();
            cm.Conflict = BLLConflicts.GetConflict(conflictId);
            if(cm.Conflict.HasArbitralClause)
            {
                cm.Clause = BLLLegalDocuments.GetClauseCompromissoire(conflictId);
            }
            return View("Clause", cm);
        }

        public ActionResult SendToOpponents(int conflictId)
        {
            var previous = BLLConflicts.GetFullConflict(conflictId);

            previous.State = (int)ConflictState.Pending;
            BLLConflicts.UpdateConflict(previous);

            BLLConflicts.MarkUserDeclarationHasCompleted(conflictId, User.Identity.GetUserId());

            if (User.Identity.GetId() == previous.IdCreationUser)
            {
                foreach (var invit in previous.Invitations)
                {
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.InvitationToJoinConflict(invit, previous, 
                        String.Format("{0}?i={1}", Request.Url.Host.ToLower() + Url.Action("Conflict", "Viewer", new { conflictId = previous.Id }), invit.Id), 
                        User.Identity.FirstName() + " " + User.Identity.LastName(),
                         Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                }
            }
            var user = BLLUsers.GetUserById(previous.IdCreationUser);
            Guid gid = Guid.NewGuid();
            FastArbitreEmails.ConfirmSendToOpponents(user.Email, Request.Url.Host.ToLower() + Url.Action("Conflict", "Viewer", new { conflictId = previous.Id }), Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = gid.ToString() }), gid);
            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        #endregion

        #region Properties
        private ApplicationSignInManager _signInManager;
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        #endregion

        #region CONFLICT MANAGEMENT

        [Route("Conflict/SendInvitations")]
        public ActionResult AddUserToConflict(List<string> emails, int conflictId)
        {
            var url = Url.Action("Conflict", "Viewer", new { conflictId = conflictId });

            List<Invitation> invits = new List<Invitation>();
            foreach (var email in emails)
            {
                var invit = BLLInvitations.AddInvitation(new Invitation() { IdConflict = conflictId, Email = email, CreationDate = DateTime.Now, Id = Guid.NewGuid() });
                invits.Add(invit);
                Guid guid = Guid.NewGuid();
                FastArbitreEmails.InvitationToJoinConflict(invit, BLLConflicts.GetConflict(conflictId), 
                    String.Format("{0}&i={1}", Request.Url.Host.ToLower() + url, invit.Id), 
                    User.Identity.FirstName() + " " + User.Identity.LastName(),
                     Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
            }
            return RedirectToAction("Manage");
        }

        #endregion
        
        [AllowAnonymous]
        [HttpPost]
        public EmptyResult SavePropositionAnswers(int eventId, List<MeetingPropositionAnswer> answers, int conflictId)
        {
            foreach (var item in answers)
            {
                BLLMeetingDoodle.AddAnswer(item);
            }
            var evt = BLLConflicts.GetEvent(eventId);
            var isAnswered = true;
            foreach (var item in evt.MeetingDebates.ElementAt(0).MeetingPropositions)
            {
                if (item.MeetingPropositionAnswers.Count < evt.Conflict.UsersInConflicts.Count)
                    isAnswered = false;
            }

            if (isAnswered)
            {
                var dateOk = evt.MeetingDebates.ElementAt(0).MeetingPropositions.Where(c => c.MeetingPropositionAnswers.All(d => d.Response)).ToList();

                /// ONE DATE IS OK
                if (dateOk.Count == 1)
                {
                    evt.MeetingDebates.ElementAt(0).Date = dateOk[0].DateTimeProposition;
                    BLLMeetingDoodle.UpdateMeetingDebate(evt.MeetingDebates.ElementAt(0));

                    //ADD CHATROOM CREATION !
                    //var chatRoom = new ClickNClaim.OpenFireConnector.chatRoom(evt.Name, evt.Description, evt.MeetingDebates.ElementAt(0).Date.Value);
                    //OpenFireConnector.Connector connector = new OpenFireConnector.Connector("http://openfire-444e60hk.cloudapp.net:9090/", "admin", "SF211084agantio");
                    //if (chatRoom.members.Where(c => c.value == conflict.Arbiter.DisplayName.Replace(" ", ".") + "@openfire").FirstOrDefault() != null)
                    //{
                    //    chatRoom.members.RemoveAll(c => c.value == conflict.Arbiter.DisplayName.Replace(" ", ".") + "@openfire");
                    //}
                    //chatRoom.owners.Add(new OpenFireConnector.owner() { value = conflict.Arbiter.DisplayName.Replace(" ", ".") + "@openfire" });
                    /////
                    var arbiter = BLLUsers.GetUserById(evt.Conflict.IdArbiterAssigned);
                    //foreach (var item in evt.Conflict.UsersInConflicts)
                    //{
                    //    var user = BLLUsers.GetUserById(item.IdUser);
                    //    if (user.DisplayName != arbiter.DisplayName)
                    //    {
                    //        var existingUserInOpenFire = connector.UserExists(user.DisplayName.Replace(" ", "."));
                    //        if (existingUserInOpenFire == null)
                    //        {
                    //            existingUserInOpenFire = connector.CreateUser(new OpenFireConnector.user() { email = user.Email, name = user.DisplayName.Replace(' ', '.'), password = user.Email, username = user.DisplayName.Replace(' ', '.') });
                    //        }

                    //        chatRoom.members.Add(new OpenFireConnector.member() { value = user.DisplayName.Replace(" ", ".").ToLower() + "@openfire" });
                    //    }
                    //}

                    //chatRoom.owners.Add(new OpenFireConnector.owner() { value = arbiter.DisplayName.Replace(" ", ".") + "@openfire" });

                    var link = "";// connector.CreateChatroom(chatRoom);

                    foreach (var item in evt.Conflict.UsersInConflicts)
                    {
                        var user = BLLUsers.GetUserById(item.IdUser);
                        var guid = Guid.NewGuid();
                        FastArbitreEmails.VisioProgrammed(user.Email, evt.IdConflict,link, evt.MeetingDebates.ElementAt(0).Title, dateOk[0],
                             Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                      
                       
                    }
                  
                 
                    Guid guidd = Guid.NewGuid();
                    FastArbitreEmails.VisioProgrammedForArbiter(arbiter.Email, evt.MeetingDebates.ElementAt(0).Title, evt.IdConflict,
                       Request.UrlReferrer.DnsSafeHost + Url.Action("Conflict", "Viewer", new { conflictId = evt.IdConflict }), dateOk[0],
                         Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guidd.ToString() }), guidd);
                }
                else if (dateOk.Count == 0)
                {
                    var arbiter = BLLUsers.GetUserById(evt.Conflict.IdArbiterAssigned);
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.VisioNotProgrammable(arbiter.Email, evt.MeetingDebates.ElementAt(0).Title, 
                        evt.IdConflict, Request.UrlReferrer.DnsSafeHost + Url.Action("Conflict", "Viewer", new { conflictId = evt.IdConflict }),
                         Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                    
                }
                else
                {
                    var arbiter = BLLUsers.GetUserById(evt.Conflict.IdArbiterAssigned);
                    Guid guid = Guid.NewGuid();
                    FastArbitreEmails.VisioMultipleDateChoice(arbiter.Email,  evt.MeetingDebates.ElementAt(0).Title, evt.IdConflict,
                        Request.UrlReferrer.DnsSafeHost + Url.Action("Conflict", "Viewer", new { conflictId = evt.IdConflict }),
                         Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                }
            }

            return new EmptyResult();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult AddNewDates(int conflictId, int eventId, List<DateTime> rdv)
        {
            var evt = BLLConflicts.GetEvent(eventId);
            var visioDebate = evt.MeetingDebates.ElementAt(0).Id;
            foreach (var item in rdv)
            {
                BLLMeetingDoodle.AddMeetingDebatePropositions(visioDebate, rdv.Select(c => new MeetingProposition() { IdMeetingDebate = visioDebate, DateTimeProposition = c }).ToList());
            }

            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        public EmptyResult SetDateForVisio(int idMeetingDebate, int idMeetingProposition, int conflictId)
        {
            var meetingDebate = BLLMeetingDoodle.GetMeetingDebate(idMeetingDebate);
            meetingDebate.Date = BLLMeetingDoodle.GetMeetingProposition(idMeetingProposition).DateTimeProposition;
            BLLMeetingDoodle.UpdateMeetingDebate(meetingDebate);
            return new EmptyResult();
        }

        public ActionResult PreConclusion(int conflictId, string preconclusion)
        {
            var conflict = BLLConflicts.GetConflictWithUsers(conflictId);
            conflict.ArbiterPreConclusion = preconclusion;
            BLLConflicts.AddConflictStateHistoric(new ConflictStateHistoric()
            {
                ConflictStateId = (int)ConflictState.PreConcluded,
                CreateDate = DateTime.Now,
                IdConflict = conflict.Id,
                ConflictStateName = MetadataHelper.GetEnumDisplayNAme(ConflictState.PreConcluded)
            });

            BLLConflicts.AddConflictStateHistoric(new ConflictStateHistoric()
            {
                ConflictStateId = (int)ConflictState.PreConclusionDebate,
                CreateDate = DateTime.Now,
                IdConflict = conflict.Id,
                CountDown = 7,
                ConflictStateName = MetadataHelper.GetEnumDisplayNAme(ConflictState.PreConclusionDebate)
            });

            conflict.State = (int)ConflictState.PreConclusionDebate;
            BLLConflicts.UpdateConflict(conflict);




            foreach (var item in conflict.UsersInConflicts)
            {
                Guid guid = Guid.NewGuid();
                FastArbitreEmails.PreConclusion(item.User.Email, item.User.DisplayName, conflict.Id, Request.UrlReferrer.DnsSafeHost + Url.Action("Conflict", "Viewer", new { conflictId = conflict.Id }), "/*FileUrl*/",
                     Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
            }

            BLLConflicts.AddEvent(new Event()
            {
                IdConflict = conflictId,
                DateBegin = DateTime.Now,
                Description = preconclusion,
                Name = "Pré-conclusion",
                IdUser = conflict.IdArbiterAssigned,
                Type = (int)EventTypeEnum.PreConclusion
            });




            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }


        public ActionResult Conclusion(int conflictId, string conclusion)
        {
            var conflict = BLLConflicts.GetConflictWithUsers(conflictId);
            conflict.ArbiterPreConclusion = conclusion;
            conflict.State = (int)ConflictState.Concluded;
            BLLConflicts.UpdateConflict(conflict);

            BLLConflicts.AddConflictStateHistoric(new ConflictStateHistoric()
            {
                IdConflict = conflictId,
                ConflictStateId = (int)ConflictState.Concluded,
                ConflictStateName = MetadataHelper.GetEnumDisplayNAme(ConflictState.Concluded),
                CreateDate = DateTime.Now,
            });


            foreach (var item in conflict.UsersInConflicts)
            {
                Guid guid = Guid.NewGuid();
                FastArbitreEmails.Sentence(item.User.Email, conflict.Id, Request.UrlReferrer.DnsSafeHost + Url.Action("Conflict", "Viewer", new { conflictId = conflict.Id }), "/*FILEURL*/",
                     Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
            }

            BLLConflicts.AddEvent(new Event()
            {
                IdConflict = conflictId,
                DateBegin = DateTime.Now,
                Description = conclusion,
                Name = "Conclusion",
                IdUser = conflict.IdArbiterAssigned,
                Type = (int)EventTypeEnum.Conclusion
            });

            return RedirectToAction("Conflict", "Viewer", new { conflictId = conflictId });
        }

        [Route("Conflict/{conflictId}/Declaration-Step1")]
        public ActionResult ConflictType(int conflictId)
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }

            ViewBag.conflictId = conflictId;
            ConflictTypeModel ctm = new ConflictTypeModel();
            ctm.Conflict = BLLConflicts.GetFullConflict(conflictId);

            if (ctm.Conflict.IdConflictType != null)
            {
                ctm.DefaultEvents = BLLRefs.ListDefaultEventsForConflictType(ctm.Conflict.IdConflictType.Value);
            }
            else if (ctm.Conflict.Events.Any(c => c.IdDefaultEvent != null))
            {
                var elt = ctm.Conflict.Events.First(c => c.IdDefaultEvent != null);
                ctm.Conflict.IdConflictType = BLLConflicts.GetDefaultEventConflictType(elt.IdDefaultEvent.Value);
                ctm.Conflict.ConflictType = BLLConflicts.GetConflictTypeById(ctm.Conflict.IdConflictType.Value);
                ctm.DefaultEvents = BLLRefs.ListDefaultEventsForConflictType(ctm.Conflict.IdConflictType.Value);
            }

            return View(ctm);
        }

        [HttpGet]
        public ContentResult Types(int? category, int conflictId)
        {
            return new ContentResult() { Content = JsonHelper.GetJsonString(BLLRefs.ListConflictsType(category)), ContentType = "application/json" };
        }

        [HttpGet]
        public ContentResult DefaultEvents(int conflictTypeId, int conflictId)
        {
            return new ContentResult() { Content = JsonHelper.GetJsonString(BLLRefs.ListDefaultEventsForConflictType(conflictTypeId)), ContentType = "application/json" };
        }

        public ActionResult SaveStep1Declaration(Conflict conflict, string otherType, int conflictId)
        {
            if (!String.IsNullOrWhiteSpace(otherType))
            {
                var conflictType = BLLRefs.AddConflictType(new Common.ConflictType() { Code = "noname", IsMapped = false, Name = otherType });
                BLLConflicts.UpdateConflictType(conflict.Id, conflictType.Id);
            }
            else if (conflict.IdConflictType != null)
            {
                BLLConflicts.UpdateConflictType(conflict.Id, conflict.IdConflictType.Value);
            }

            //var previous = BLLConflicts.GetFullConflict(conflict.Id);

            //var toDelete = previous.Events.Where(c => c.IdDefaultEvent != null  && !conflict.Events.Any(d => d.Id == c.Id)).ToList();
            //foreach (var item in toDelete)
            //{
            //    BLLConflicts.RemoveEvent(item.Id);
            //}


            //foreach (var item in conflict.Events)
            //{
            //    if (item.Id > 0)
            //    {
            //        var evt = BLLConflicts.GetEvent(item.Id);
            //        evt.DateBegin = item.DateBegin;
            //        evt.Description = item.Description;
            //        BLLConflicts.UpdateEvent(evt);
            //    }
            //    else
            //    {
            //        item.IdUser = User.Identity.GetId();
            //        item.IdConflict = conflict.Id;
            //        BLLConflicts.AddEvent(item);
            //    }
            //}

            return RedirectToAction("Timeline", new { conflictId = conflict.Id });
        }

        public ContentResult AddFreeComment(int conflictId, string comment)
        {
            Event e = new Event();
            e.DateBegin = DateTime.Now;
            e.Description = comment;
            e.IdConflict = conflictId;
            e.IdUser = User.Identity.GetId();
            e.Name = "Commentaire libre";
            e.Type = (int)EventTypeEnum.FreeComment;
            e = BLLConflicts.AddEvent(e);

            return new ContentResult() { Content = JsonHelper.GetJsonString(e), ContentType = "application/json" };

        }


    }



    public partial class ConflictController
    {
        [Route("Conflict/TestPieces/{id:int}")]
        public ActionResult TestPieces(int conflictId)
        {
            var conflict = BLLConflicts.GetFullConflict(conflictId);
            return View("Pieces", conflict);

        }


    }
}