using System.Web.Mvc;
using Veb_Projekat.DataServices.UserDataService;

namespace Veb_Projekat.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserDataService _dataService = new UserDataService();
        public ActionResult Index()
        {
            var users = _dataService.LoadUsers();
            var arrangements = _dataService.LoadArrangements(users);
            var reservations = _dataService.LoadReservations(users, arrangements);

            ViewBag.Users = users;
            ViewBag.Arrangements = arrangements;
            ViewBag.Reservations = reservations;

            return View();
        }

        /*public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }*/
    }
}