using ClickNClaim.WebPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace ClickNClaim.WebPortal.Extensions
{
    public static class IdentityExtensions
    {

        public static string GetId(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("Id") != null)
                return ((ClaimsIdentity)identity).FindFirst("Id").Value;
            else
                return String.Empty; 
        }

        public static string FirstName(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("FirstName") != null)
                return ((ClaimsIdentity)identity).FindFirst("FirstName").Value;
            else
                return String.Empty;
        }

        public static string LastName(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("LastName") != null)
                return ((ClaimsIdentity)identity).FindFirst("LastName").Value;
            else
                return String.Empty;
        }

        public static string Email(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("Email") != null)
                return ((ClaimsIdentity)identity).FindFirst("Email").Value;
            else
                return String.Empty;
        }
       
    }
}