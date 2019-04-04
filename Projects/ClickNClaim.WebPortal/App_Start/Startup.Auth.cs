using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using ClickNClaim.WebPortal.Models;
using Microsoft.Owin.Security.Facebook;
using System.Threading.Tasks;
using ClickNClaim.WebPortal.App_Start;
using Owin.Security.Providers.LinkedIn;

namespace ClickNClaim.WebPortal
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");



            var facebookOptions = new Microsoft.Owin.Security.Facebook.FacebookAuthenticationOptions()
            {
                Provider = new FacebookAuthenticationProvider()
                {
                    OnAuthenticated = (context) =>
                    {
                        var rawUserObjectFromFacebookAsJson = context.User;
                        context.Identity.AddClaim(new System.Security.Claims.Claim("email", context.User["email"].ToString()));
                        context.Identity.AddClaim(new System.Security.Claims.Claim("first_name", context.User["first_name"].ToString()));
                        context.Identity.AddClaim(new System.Security.Claims.Claim("last_name", context.User["last_name"].ToString()));
                        context.Identity.AddClaim(new System.Security.Claims.Claim("id", context.User["id"].ToString()));
                        return Task.FromResult(0);
                    }
                },
                AppId = "158323397941161",
                AppSecret = "7edd8e6417e7820413fe945834c21727",
            };
            facebookOptions.BackchannelHttpHandler = new FacebookBackChannelHandler();
            facebookOptions.UserInformationEndpoint = "https://graph.facebook.com/v2.4/me?fields=id,name,email,first_name,last_name";

            app.UseFacebookAuthentication(facebookOptions);

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "351941393609-1dknnn6g0oljipqd9hit39cdnegr8jjd.apps.googleusercontent.com",
                ClientSecret = "nAS7WVkesG8FZD4ETxCrv7cv"
            });

            app.UseLinkedInAuthentication(clientId: "78uixyboq22bzd", clientSecret: "egvmg5fb8cP4nsny");

        }
    }
}