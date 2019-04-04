using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ClickNClaim.WebPortal.Models;
using ClickNClaim.Business;
using System.Configuration;
using ClickNClaim.WebPortal.Helpers;
using ClickNClaim.Common;
using ClickNClaim.WebPortal.Tools;
using ClickNClaim.WebPortal.Extensions;

namespace ClickNClaim.WebPortal.Controllers
{
   // [RequireHttps]
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;


        public ActionResult UpdatePhoto(HttpPostedFileBase PhotoFile) 
        {
            if (PhotoFile!= null && PhotoFile.ContentLength > 0)
            {
              var file = AzureFileHelper.AddPhoto(User.Identity.GetUserId(), PhotoFile);
                var user = BLLUsers.GetUserById(User.Identity.GetUserId());
                user.PhotoPath = file.Uri.AbsoluteUri;
                BLLUsers.UpdateUser(user);
              
            }
            return RedirectToAction("Profil");
        }

        public ActionResult UpdateUserPersonalInfo(AspNetUser user)
        {
            if(BLLUsers.GetUserByEmail(user.Email) != null && BLLUsers.GetUserByEmail(user.Email).Id != User.Identity.GetId())
            {
                TempData["Error"] = "Adresse mail déjà existante, vous ne pouvez pas utiliser la même adresse mail pour plusieurs compte utilisateur";
                return RedirectToAction("Profil");
            }
            BLLUsers.UpdateUserPersonalInfo(user);
            return RedirectToAction("Profil");
        }

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // find user by username first
            var user =  UserManager.FindByEmail(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Nom d'utilisateur ou mot de passe erroné, veuillez ressayer.");
                return View(model);
            }

            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                ModelState.AddModelError("", string.Format("Quelqu'un a tenté de se connecter à votre compte sans succès. Afin d'empêcher les personnes malveillantes de trouver votre mot de passe en testant un maximum de combinaison (technique dite 'bruteforce'), votre compte est maintenant bloqué pour {0} minutes.", ConfigurationManager.AppSettings["DefaultAccountLockoutTimeSpan"].ToString()));
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    if (returnUrl != null)
                    {
                        if ((returnUrl.Contains("userId") || returnUrl.Contains("?i=")))
                        {
                            returnUrl = returnUrl.Replace("userId", user.Id);
                            var invitation = HttpUtility.ParseQueryString(returnUrl)[0];
                            BLLConflicts.AddUserInConflictFromInvitation(user.Email, Guid.Parse(invitation), user.Id);
                        }
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Profil");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Nom d'utilisateur ou mot de passe erroné, veuillez ressayer.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string returnUrl)
        {
            RegisterViewModel model = new RegisterViewModel();

            if (!String.IsNullOrWhiteSpace(Request.QueryString.Get("i")))
            {
                var invit = BLLInvitations.GetInvitation(Guid.Parse(Request.QueryString.Get("i")));
                model.FirstName = invit.FirstName;
                model.LastName = invit.LastName;
                model.Email = invit.Email;
            }

            return View(model);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, CreateDate = DateTime.Now, FirstName = model.FirstName, LastName = model.LastName };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "User");
                    if (!String.IsNullOrWhiteSpace(Request.QueryString.Get("i")))
                    {
                        BLLUsers.AutoConfirmUser(user.Id);
                        var invit = Guid.Parse(Request.QueryString.Get("i"));
                        var invitObj = BLLInvitations.GetInvitation(invit);
                        BLLConflicts.AddUserInConflictFromInvitation(user.Email, invit, user.Id);
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        OpenFireConnector.Connector connector = new OpenFireConnector.Connector("http://openfire-444e60hk.cloudapp.net:9090/", "admin", "SF211084agantio");
                        connector.CreateUser(new OpenFireConnector.user() { password = user.Email, username = user.FirstName.Replace(" ", ".").ToLower() + "." + user.LastName.Replace(" ", ".").ToLower(), email = user.Email });

                        return RedirectToAction("Identification", "Conflict", new { conflictId = invitObj.IdConflict, idUser = user.Id });
                    }
                    else
                    {
                        BLLUsers.AutoConfirmUser(user.Id);
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        if(!String.IsNullOrWhiteSpace(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }

                       // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                       // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                       // Guid guid = Guid.NewGuid();
                       // FastArbitreEmails.ConfirmAccount(model.Email, model.FirstName + " " + model.LastName, callbackUrl, Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
                       //// await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                       // return View("ShouldConfirmEmail");
                    }
                    
                    // Send an email with this link
                   
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code, string captchaConfirm)
        {
            ViewBag.userId = userId;
            ViewBag.code = code;

           
               
                var result = await UserManager.ConfirmEmailAsync(userId, code);
                if (result.Succeeded)
                {
                    if (Session["oi3_C309"] != null)
                    {
                        var user = await UserManager.FindByEmailAsync(Session["oi3_C309"].ToString());
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        OpenFireConnector.Connector connector = new OpenFireConnector.Connector("http://openfire-444e60hk.cloudapp.net:9090/", "admin", "SF211084agantio");
                        connector.CreateUser(new OpenFireConnector.user() { password = user.Email, username = user.FirstName.ToLower() + "."+ user.LastName.ToLower(), email = user.Email });

                        return RedirectToAction("Create", "Conflict", new { firstname = user.FirstName, lastname = user.LastName, email = user.Email });
                    }
                }
                if (result.Succeeded)
            {
                
                var user = UserManager.FindById(userId);
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return RedirectToAction("Profil");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [AllowAnonymous]
        public ActionResult ShouldConfirmEmail()
        {
            return View();
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null /*|| !(await UserManager.IsEmailConfirmedAsync(user.Id))*/)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                Guid guid = Guid.NewGuid();
                FastArbitreEmails.ReinitiatingPassword(user.Email, callbackUrl, Request.UrlReferrer.DnsSafeHost + Url.Action("Index", "Email", new { id = guid.ToString() }), guid);
              //  await UserManager.SendEmailAsync(user.Id, "Réinitialisation de votre mot de passe", "Merci de réinitialiser votre mot de passe en cliquant sur ce <a href=\"" + callbackUrl + "\">lien</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

       
        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    if (!String.IsNullOrWhiteSpace(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Profil", "Account");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;

                    ApplicationUser au = null;

                    if (loginInfo.Login.LoginProvider.ToLower() == "facebook")
                    {
                        au = new ApplicationUser
                        {
                            UserName = loginInfo.Email,
                            Email = loginInfo.Email,
                            CreateDate = DateTime.Now,
                            FirstName = loginInfo.ExternalIdentity.Claims.First(x => x.Type == "first_name").Value,
                            LastName = loginInfo.ExternalIdentity.Claims.First(x => x.Type == "last_name").Value,
                        };
                    }
                    else
                    {
                        au = new ApplicationUser
                        {
                            UserName = loginInfo.ExternalIdentity.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value,
                            Email = loginInfo.ExternalIdentity.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value,
                            CreateDate = DateTime.Now,
                            FirstName = loginInfo.ExternalIdentity.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").Value,
                            LastName = loginInfo.ExternalIdentity.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").Value,
                        };
                    }

                    var existingUser = BLLUsers.GetUserByEmail(loginInfo.Email);
                    if (existingUser != null)
                    {
                        BLLUsers.AssociateUserAccounts(loginInfo.Login.LoginProvider, loginInfo.Login.ProviderKey, existingUser.Id);

                        if (loginInfo.Login.LoginProvider == "facebook")
                        {
                            existingUser.PhotoPath = String.Format("http://graph.facebook.com/{0}/picture", loginInfo.ExternalIdentity.Claims.First(x => x.Type == "id").Value);
                            BLLUsers.UpdateUser(existingUser);
                        }
                        au.Id = existingUser.Id;
                        await SignInManager.SignInAsync(au, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Profil", "Account");
                    }


                    var res = await UserManager.CreateAsync(au);
                    if (res.Succeeded)
                    {
                        res = await UserManager.AddLoginAsync(au.Id, loginInfo.Login);
                        var u = BLLUsers.GetUserById(au.Id);
                        if (loginInfo.Login.LoginProvider == "facebook")
                        {
                            u.PhotoPath = String.Format("http://graph.facebook.com/{0}/picture", loginInfo.ExternalIdentity.Claims.First(x => x.Type == "id").Value);
                            BLLUsers.UpdateUser(u);
                        }
                        if (res.Succeeded)
                        {
                            await SignInManager.SignInAsync(au, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Profil", "Account");
                        }
                        else
                        {
                            return View("ExternalLoginFailure");
                        }
                    }
                    else
                    {
                        return View("ExternalLoginFailure");
                    }
              
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, CreateDate = DateTime.Now };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Profil(int? page)
        {

            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            ViewBag.Page = page ?? 0;
            return View(BLLUsers.GetMyProfil(User.Identity.GetUserId()));
        }

        [HttpPost]
        public ActionResult UpdateCompany(Company c)
        {
            var company = BLLUsers.AddOrUpdateCompany(c);
            BLLUsers.UpdateUserMainCompany(company.Id, User.Identity.GetUserId());
            return RedirectToAction("Profil");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}