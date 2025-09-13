using System;
using System.Web.Mvc;
using Veb_Projekat.Services;

namespace Veb_Projekat.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string name = "", string location = "", string type = "", string transport = "", DateTime? startDateFrom = null, DateTime? startDateTo = null,
            DateTime? endDateFrom = null, DateTime? endDateTo = null, string sortBy = "", string sortDir = "asc")
        {
            var arrangements = ArrangementService.SearchArrangements(name, location, type, transport, startDateFrom, startDateTo, endDateFrom, endDateTo);

            switch (sortBy)
            {
                case "Name":
                    arrangements = ArrangementService.SortByName(arrangements, sortDir == "asc");
                    break;
                case "StartDate":
                    arrangements = ArrangementService.SortByStartDate(arrangements, sortDir == "asc");
                    break;
                case "EndDate":
                    arrangements = ArrangementService.SortByEndDate(arrangements, sortDir == "asc");
                    break;
                default:
                    break;
            }

            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;

            ViewBag.SelectedName = name;
            ViewBag.SelectedLocation = location;
            ViewBag.SelectedType = type;
            ViewBag.SelectedTransport = transport;
            ViewBag.SelectedStartFrom = startDateFrom?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.SelectedStartTo = startDateTo?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.SelectedEndFrom = endDateFrom?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.SelectedEndTo = endDateTo?.ToString("yyyy-MM-dd") ?? "";

            ViewBag.ArrangementTypes = Enum.GetValues(typeof(Veb_Projekat.Models.Enums.ArrangementTypeEnum));
            ViewBag.TransportTypes = Enum.GetValues(typeof(Veb_Projekat.Models.Enums.TransportTypeEnum));
            ViewBag.Arrangements = arrangements;

            return View();
        }

        public ActionResult Details(int id)
        {
            var arrangement = ArrangementService.GetDetails(id);
            if (arrangement == null)
                return HttpNotFound();

            return View(arrangement);
        }
    }
}
