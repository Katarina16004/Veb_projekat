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

        public ActionResult Details(int id, string accName = "", string accType = "", bool? hasPool = null, bool? hasSpa = null, bool? accessible = null, bool? hasWifi = null, string accSortBy = "", string accSortDir = "asc")
        {
            var arrangement = ArrangementService.GetDetails(id);
            if (arrangement == null)
                return HttpNotFound();

            // Filtriraj i sortira accommodations
            var filteredAccommodations = AccommodationService.SearchAccommodations(
                arrangement.Accommodations,
                accType,
                accName,
                hasPool,
                hasSpa,
                accessible,
                hasWifi
            );

            switch (accSortBy)
            {
                case "Name":
                    filteredAccommodations = AccommodationService.SortByName(filteredAccommodations, accSortDir == "asc");
                    break;
                case "TotalUnits":
                    filteredAccommodations = AccommodationService.SortByTotalUnits(filteredAccommodations, accSortDir == "asc");
                    break;
                case "AvailableUnits":
                    filteredAccommodations = AccommodationService.SortByAvailableUnits(filteredAccommodations, accSortDir == "asc");
                    break;
            }

            // Sačuvaj u arrangement da View i dalje prima Arrangement model
            arrangement.Accommodations = filteredAccommodations;

            // Prosledi filter/sort vrednosti u ViewBag da zadrži formu
            ViewBag.AccName = accName;
            ViewBag.AccType = accType;
            ViewBag.HasPool = hasPool;
            ViewBag.HasSpa = hasSpa;
            ViewBag.Accessible = accessible;
            ViewBag.HasWifi = hasWifi;
            ViewBag.AccSortBy = accSortBy;
            ViewBag.AccSortDir = accSortDir;

            return View(arrangement); // <-- šaljemo ceo Arrangement
        }

    }
}
