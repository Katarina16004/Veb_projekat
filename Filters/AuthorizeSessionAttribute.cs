using System;
using System.Web;
using System.Web.Mvc;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Filters
{
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public RoleEnum[] AllowedRoles { get; set; }

        public AuthorizeSessionAttribute(params RoleEnum[] roles)
        {
            AllowedRoles = roles ?? new RoleEnum[0];
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionUser sessionUser = HttpContext.Current.Session["CurrentUser"] as SessionUser;

            if (sessionUser == null || !sessionUser.IsLoggedIn)
            {
                filterContext.Controller.TempData["Error"] = "You must be logged in to access this page.";
                filterContext.Result = new RedirectResult("/Account/Login");
                return;
            }

            if (AllowedRoles.Length > 0)
            {
                bool hasPermission = false;
                foreach (var role in AllowedRoles)
                {
                    if (sessionUser.UserRole == role)
                    {
                        hasPermission = true;
                        break;
                    }
                }

                if (!hasPermission)
                {
                    filterContext.Controller.TempData["Error"] = "You don't have permission to access this page.";
                    filterContext.Result = new RedirectResult("/Home/Index");
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}