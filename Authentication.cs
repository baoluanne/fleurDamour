using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using fleurDamour.Models;
using System;
using System.Linq;

namespace fleurDamour
{
    public class LoginState
    {
        /*public static int WrongPassNum = 0;
        public static DateTime? WrongPassCD = DateTime.Now;*/
        public static string OTPCode = null;

        public static bool IsLogged(string User)
        {
            if (string.IsNullOrEmpty(User))
            {
                return false;
            }

            using FleurDamourContext db = new();
            return db.Accounts.Any(a => a.UserName == User);
        }

        public static Account UserAcc(string User)
        {
            if (string.IsNullOrEmpty(User))
            {
                return null;
            }

            using FleurDamourContext db = new();
            return db.Accounts.SingleOrDefault(a => a.UserName == User);
        }

        /* public static bool CheckLock(Account User)
         {
             return DateTime.Now < User.LockDate;
         }*/

        /*public static bool CheckCoolDown()
        {
            return DateTime.Now < WrongPassCD;
        }

        public static void SetWrongPassCD(int seconds)
        {
            WrongPassCD = DateTime.Now.AddSeconds(seconds);
        }*/
    }
    public class CheckAdminRole : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string userName = context.HttpContext.Session.GetString("UserName");
            Account UserAcc = LoginState.UserAcc(userName);
            if (context.HttpContext.Session.GetString("Role") == null || !context.HttpContext.Session.GetString("Role").Equals("Admin") || UserAcc.Role != "Admin")
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"Area","User"},
                        {"Controller","Home"},
                        {"Action","Index"}
                    });
            }
        }
    }

    public class Authentication : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("UserName") == null)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"Controller","Account" },
                        {"Action","Login"}
                    });
            }
        }
    }
}
