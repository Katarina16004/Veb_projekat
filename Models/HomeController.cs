using System;
using System.Linq;
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

        public ActionResult Details(int id, string accName = "", string accType = "", bool? hasPool = null, bool? hasSpa = null, bool? accessible = null, 
            bool? hasWifi = null, string accSortBy = "", string accSortDir = "asc")
        {
            var arrangement = ArrangementService.GetDetails(id);
            if (arrangement == null)
                return HttpNotFound();

            var filteredAccommodations = AccommodationService.SearchAccommodations(arrangement.Accommodations, accType, accName,
                hasPool, hasSpa, accessible, hasWifi);

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

            foreach (var acc in filteredAccommodations)
            {
                if (acc.Units == null) continue;
                //po smestaju
                string qMinGuests = Request.QueryString["minGuests_" + acc.Id];
                string qMaxGuests = Request.QueryString["maxGuests_" + acc.Id];
                string qPetsAllowed = Request.QueryString["petsAllowed_" + acc.Id];
                string qMaxPrice = Request.QueryString["maxPrice_" + acc.Id];
                string qUnitSortBy = Request.QueryString["unitSortBy_" + acc.Id];
                string qUnitSortDir = Request.QueryString["unitSortDir_" + acc.Id];

                int? minG = int.TryParse(qMinGuests, out int mg) ? mg : (int?)null;
                int? maxG = int.TryParse(qMaxGuests, out int xg) ? xg : (int?)null;
                bool? pets = bool.TryParse(qPetsAllowed, out bool p) ? p : (bool?)null;
                decimal? maxP = decimal.TryParse(qMaxPrice, out decimal mp) ? mp : (decimal?)null;

                var units = AccommodationUnitService.SearchUnits(acc.Units, minG, maxG, pets, maxP);

                switch (qUnitSortBy)
                {
                    case "MaxGuests":
                        units = AccommodationUnitService.SortByMaxGuests(units, qUnitSortDir != "desc");
                        break;
                    case "Price":
                        units = AccommodationUnitService.SortByPrice(units, qUnitSortDir != "desc");
                        break;
                }

                acc.Units = units;
            }

            arrangement.Accommodations = filteredAccommodations;
            return View(arrangement);
        }
    }
}
