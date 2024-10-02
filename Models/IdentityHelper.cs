using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace AmazonCognito.Models
{
    public static class IdentityHelper
    {
        public static bool IsAuthenticated()
        {
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return true;
            }

            return false;
        }

        public static bool IsAuthorized(params string[] page_authorized_roles)
        {
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                List<string> userRoles = HttpContext.Current.User.Identity.GetRole();

                if (userRoles != null && userRoles.Count > 0)
                {
                    foreach (var role in userRoles)
                    {
                        if (page_authorized_roles.Contains(role.ToString(), StringComparer.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static object GetUserClaims()
        {
            return ClaimsPrincipal.Current.Identities.First().Claims.ToList();
        }

        public static void SignIn(User user, Authenticate.Token token, params string[] roles)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.PrimarySid, user.Id),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.GivenName, user.GivenName),
                    new Claim(ClaimTypes.Surname, user.FamilyName),
                    new Claim(ClaimTypes.Email, user.Email),
                };

                foreach (var item in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, item));
                }


                var claimsIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

                //This uses OWIN authentication
                LoggingHelper.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                LoggingHelper.AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, claimsIdentity);

                HttpContext.Current.User = new ClaimsPrincipal(LoggingHelper.AuthenticationManager.AuthenticationResponseGrant.Principal);
            }
            catch (Exception ex)
            {
            }

            HttpContext.Current.Session["User_Object"] = user;
            HttpContext.Current.Session["AuthenticationToken"] = token;
        }

        //public static void UpdateSignInClaims(User user)
        //{
        //    ClaimsIdentity identity = (ClaimsIdentity)HttpContext.Current.User.Identity;

        //    var claims = new List<Claim>
        //        {
        //            new Claim(ClaimTypes.PrimarySid, user.Id),
        //            new Claim(ClaimTypes.Name, user.Name),
        //            new Claim(ClaimTypes.GivenName, user.GivenName),
        //            new Claim(ClaimTypes.Surname, user.FamilyName),
        //            new Claim(ClaimTypes.Email, user.Email),
        //        };

        //    identity.

        //    Claim claim = identity.FindFirst(ClaimTypes.Name);
        //    identity.RemoveClaim(claim);
        //    identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));

        //    //This uses OWIN authentication
        //    LoggingHelper.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        //    LoggingHelper.AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, identity);

        //    HttpContext.Current.User = new ClaimsPrincipal(LoggingHelper.AuthenticationManager.AuthenticationResponseGrant.Principal);
        //    HttpContext.Current.Session["User_Object"] = user;
        //}
    }

    public static class IdentityExtensions
    {
        public static List<string> GetRole(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                return ci.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            }
            return null;
        }
    }

    public static class LoggingHelper
    {
        public static IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }
    }
}