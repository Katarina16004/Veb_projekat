using System.Web.Mvc;
using Veb_Projekat.Models;

namespace Veb_Projekat.Controllers
{
    public class BaseController : Controller
    {
        protected SessionUser CurrentUser => Session["CurrentUser"] as SessionUser;

        protected bool IsLoggedIn => CurrentUser?.IsLoggedIn == true;

        protected bool IsTourist => CurrentUser?.UserRole == Models.Enums.RoleEnum.Tourist;

        protected bool IsManager => CurrentUser?.UserRole == Models.Enums.RoleEnum.Manager;

        protected bool IsAdmin => CurrentUser?.UserRole == Models.Enums.RoleEnum.Administrator;
    }
}