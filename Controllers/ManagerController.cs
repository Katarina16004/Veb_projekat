using System.Web.Mvc;
using Veb_Projekat.Filters;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Controllers
{
    [AuthorizeSession(RoleEnum.Manager)]
    public class ManagerController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateArrangement()
        {
            return View();
        }

        public ActionResult ApproveComments()
        {
            return View();
        }
    }
}