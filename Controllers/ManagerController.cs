using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Veb_Projekat.Filters;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;
using Veb_Projekat.Services;

namespace Veb_Projekat.Controllers
{
    [AuthorizeSession(RoleEnum.Manager)]
    public class ManagerController : BaseController
    {
        #region Dashboard and Overview

        public ActionResult Index()
        {
            var myArrangements = ArrangementService.GetByManager(CurrentUser.Username);

            ViewBag.MyArrangements = myArrangements;
            ViewBag.TotalArrangements = myArrangements.Count;
            ViewBag.UpcomingArrangements = myArrangements.Count(a => a.StartDate > DateTime.Now);
            ViewBag.PastArrangements = myArrangements.Count(a => a.StartDate <= DateTime.Now);

            return View();
        }

        #endregion

        #region Arrangement Management

        // GET: CreateArrangement
        public ActionResult CreateArrangement()
        {
            ViewBag.ArrangementTypes = Enum.GetValues(typeof(ArrangementTypeEnum));
            ViewBag.TransportTypes = Enum.GetValues(typeof(TransportTypeEnum));
            return View();
        }

        // POST: CreateArrangement
        [HttpPost]
        public ActionResult CreateArrangement(string name, ArrangementTypeEnum type, TransportTypeEnum transport,
            string location, DateTime startDate, DateTime endDate, int maxPassengers,
            string description, string program)
        {
            if (!ArrangementService.CreateArrangement(CurrentUser.Username, name, type, transport, location,
                startDate, endDate, maxPassengers, description, program, out string errorMessage))
            {
                ModelState.AddModelError("", errorMessage);
                ViewBag.ArrangementTypes = Enum.GetValues(typeof(ArrangementTypeEnum));
                ViewBag.TransportTypes = Enum.GetValues(typeof(TransportTypeEnum));
                return View();
            }

            TempData["Success"] = $"Arrangement '{name}' created successfully!";
            return RedirectToAction("Index","Home");
        }

        // GET: EditArrangement
        public ActionResult EditArrangement(int id)
        {
            var arrangement = ArrangementService.GetArrangementForEdit(id, CurrentUser.Username);

            if (arrangement == null)
            {
                TempData["Error"] = "Arrangement not found or you don't have permission to edit it.";
                return RedirectToAction("Index", "Home");
            }

            if (!ArrangementService.CanEditArrangement(id, CurrentUser.Username, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ArrangementTypes = Enum.GetValues(typeof(ArrangementTypeEnum));
            ViewBag.TransportTypes = Enum.GetValues(typeof(TransportTypeEnum));

            return View(arrangement);
        }

        // POST: EditArrangement
        [HttpPost]
        public ActionResult EditArrangement(int id, string name, ArrangementTypeEnum type, TransportTypeEnum transport,
            string location, DateTime startDate, DateTime endDate, int maxPassengers,
            string description, string program)
        {
            if (!ArrangementService.UpdateArrangement(id, CurrentUser.Username, name, type, transport, location,
                startDate, endDate, maxPassengers, description, program, out string errorMessage))
            {
                ModelState.AddModelError("", errorMessage);

                var arrangement = ArrangementService.GetArrangementForEdit(id, CurrentUser.Username);
                ViewBag.ArrangementTypes = Enum.GetValues(typeof(ArrangementTypeEnum));
                ViewBag.TransportTypes = Enum.GetValues(typeof(TransportTypeEnum));

                return View(arrangement);
            }

            TempData["Success"] = $"Arrangement '{name}' updated successfully!";
            return RedirectToAction("Index", "Home");
        }

        // POST: DeleteArrangement
        [HttpPost]
        public ActionResult DeleteArrangement(int id)
        {
            if (!ArrangementService.DeleteArrangement(id, CurrentUser.Username, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
            }
            else
            {
                TempData["Success"] = "Arrangement deleted successfully.";
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: ViewAllArrangements
        public ActionResult ViewAllArrangements(bool includeDeleted = false, string name = "", string location = "",
            string type = "", string transport = "", DateTime? startDateFrom = null, DateTime? startDateTo = null,
            DateTime? endDateFrom = null, DateTime? endDateTo = null, string sortBy = "", string sortDir = "asc")
        {
            try
            {
                var arrangements = ArrangementService.GetAllByManager(CurrentUser.Username, includeDeleted);

                if (arrangements == null)
                    arrangements = new List<Arrangement>();

                arrangements = ArrangementService.SearchManagerArrangements(arrangements, name, location, type, transport,
                    startDateFrom, startDateTo, endDateFrom, endDateTo);

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
                }

                ViewBag.AllArrangements = arrangements ?? new List<Arrangement>();
                ViewBag.IncludeDeleted = includeDeleted;
                ViewBag.ActiveCount = arrangements?.Count(a => !a.IsDeleted) ?? 0;
                ViewBag.DeletedCount = arrangements?.Count(a => a.IsDeleted) ?? 0;

                ViewBag.SelectedName = name ?? "";
                ViewBag.SelectedLocation = location ?? "";
                ViewBag.SelectedType = type ?? "";
                ViewBag.SelectedTransport = transport ?? "";
                ViewBag.SelectedStartFrom = startDateFrom?.ToString("yyyy-MM-dd") ?? "";
                ViewBag.SelectedStartTo = startDateTo?.ToString("yyyy-MM-dd") ?? "";
                ViewBag.SelectedEndFrom = endDateFrom?.ToString("yyyy-MM-dd") ?? "";
                ViewBag.SelectedEndTo = endDateTo?.ToString("yyyy-MM-dd") ?? "";
                ViewBag.SortBy = sortBy ?? "";
                ViewBag.SortDir = sortDir ?? "asc";

                return View();
            }
            catch (Exception ex)
            {
                // Debug
                TempData["Error"] = "Error loading arrangements: " + ex.Message;
                ViewBag.AllArrangements = new List<Arrangement>();
                ViewBag.ActiveCount = 0;
                ViewBag.DeletedCount = 0;
                return View();
            }
        }
        #endregion

        #region Accommodation Management

        // GET: ManageAccommodations
        public ActionResult ManageAccommodations(int arrangementId)
        {
            var arrangement = ArrangementService.GetArrangementForEdit(arrangementId, CurrentUser.Username);

            if (arrangement == null)
            {
                TempData["Error"] = "Arrangement not found or you don't have permission to manage it.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Arrangement = arrangement;
            ViewBag.Accommodations = arrangement.Accommodations;

            return View();
        }

        // GET: CreateAccommodation
        public ActionResult CreateAccommodation(int arrangementId)
        {
            var arrangement = ArrangementService.GetArrangementForEdit(arrangementId, CurrentUser.Username);

            if (arrangement == null)
            {
                TempData["Error"] = "Arrangement not found or you don't have permission to add accommodations.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ArrangementId = arrangementId;
            ViewBag.ArrangementName = arrangement.Name;
            ViewBag.AccommodationTypes = Enum.GetValues(typeof(AccommodationTypeEnum));

            return View();
        }

        // POST: CreateAccommodation
        [HttpPost]
        public ActionResult CreateAccommodation(int arrangementId, string name, AccommodationTypeEnum type,
            int stars, bool hasPool, bool hasSpa, bool accessible, bool hasWifi)
        {
            if (!AccommodationService.CreateAccommodation(CurrentUser.Username, arrangementId, name, type,
                stars, hasPool, hasSpa, accessible, hasWifi, out string errorMessage))
            {
                ModelState.AddModelError("", errorMessage);

                var arrangement = ArrangementService.GetArrangementForEdit(arrangementId, CurrentUser.Username);
                ViewBag.ArrangementId = arrangementId;
                ViewBag.ArrangementName = arrangement?.Name ?? "Unknown";
                ViewBag.AccommodationTypes = Enum.GetValues(typeof(AccommodationTypeEnum));

                return View();
            }

            TempData["Success"] = $"Accommodation '{name}' created successfully!";
            return RedirectToAction("ManageAccommodations", new { arrangementId });
        }

        // GET: EditAccommodation
        public ActionResult EditAccommodation(int id)
        {
            var accommodation = AccommodationService.GetAccommodationForEdit(id, CurrentUser.Username);

            if (accommodation == null)
            {
                TempData["Error"] = "Accommodation not found or you don't have permission to edit it.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.AccommodationTypes = Enum.GetValues(typeof(AccommodationTypeEnum));

            return View(accommodation);
        }

        // POST: EditAccommodation
        [HttpPost]
        public ActionResult EditAccommodation(int id, string name, AccommodationTypeEnum type,
            int stars, bool hasPool, bool hasSpa, bool accessible, bool hasWifi)
        {
            if (!AccommodationService.UpdateAccommodation(id, CurrentUser.Username, name, type,
                stars, hasPool, hasSpa, accessible, hasWifi, out string errorMessage))
            {
                ModelState.AddModelError("", errorMessage);

                var accommodation = AccommodationService.GetAccommodationForEdit(id, CurrentUser.Username);
                ViewBag.AccommodationTypes = Enum.GetValues(typeof(AccommodationTypeEnum));

                return View(accommodation);
            }

            TempData["Success"] = $"Accommodation '{name}' updated successfully!";

            var arrangements = ArrangementRepository.GetAllByManagerUsername(CurrentUser.Username);
            var parentArrangement = arrangements.FirstOrDefault(arr => arr.Accommodations.Any(acc => acc.Id == id));

            if (parentArrangement != null)
                return RedirectToAction("ManageAccommodations", new { arrangementId = parentArrangement.Id });
            else
                return RedirectToAction("Index", "Home");
        }

        // POST: DeleteAccommodation
        [HttpPost]
        public ActionResult DeleteAccommodation(int id)
        {
            var arrangements = ArrangementRepository.GetAllByManagerUsername(CurrentUser.Username);
            var parentArrangement = arrangements.FirstOrDefault(arr =>
                arr.Accommodations.Any(acc => acc.Id == id));

            if (!AccommodationService.DeleteAccommodation(id, CurrentUser.Username, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
            }
            else
            {
                TempData["Success"] = "Accommodation deleted successfully.";
            }

            if (parentArrangement != null)
                return RedirectToAction("ManageAccommodations", new { arrangementId = parentArrangement.Id });
            else
                return RedirectToAction("Index", "Home");
        }

        // GET: ViewAllAccommodations
        public ActionResult ViewAllAccommodations(string accName = "", string accType = "", bool? hasPool = null,
            bool? hasSpa = null, bool? accessible = null, bool? hasWifi = null,
            string accSortBy = "", string accSortDir = "asc")
        {
            var accommodations = AccommodationService.GetByManager(CurrentUser.Username);

            accommodations = AccommodationService.SearchAccommodations(accommodations, accType, accName, hasPool, hasSpa, accessible, hasWifi);

            switch (accSortBy)
            {
                case "Name":
                    accommodations = AccommodationService.SortByName(accommodations, accSortDir == "asc");
                    break;
                case "TotalUnits":
                    accommodations = AccommodationService.SortByTotalUnits(accommodations, accSortDir == "asc");
                    break;
                case "AvailableUnits":
                    accommodations = AccommodationService.SortByAvailableUnits(accommodations, accSortDir == "asc");
                    break;
            }

            ViewBag.MyAccommodations = accommodations;
            ViewBag.TotalAccommodations = accommodations.Count;
            ViewBag.ActiveAccommodations = accommodations.Count(a => !a.IsDeleted);
            ViewBag.DeletedAccommodations = accommodations.Count(a => a.IsDeleted);

            ViewBag.SelectedName = accName;
            ViewBag.SelectedType = accType;
            ViewBag.HasPool = hasPool;
            ViewBag.HasSpa = hasSpa;
            ViewBag.Accessible = accessible;
            ViewBag.HasWifi = hasWifi;
            ViewBag.SortBy = accSortBy;
            ViewBag.SortDir = accSortDir;

            return View();
        }

        #endregion

        #region Accommodation Unit Management

        // GET: ManageUnits
        public ActionResult ManageUnits(int accommodationId)
        {
            var accommodation = AccommodationService.GetAccommodationForEdit(accommodationId, CurrentUser.Username);

            if (accommodation == null)
            {
                TempData["Error"] = "Accommodation not found or you don't have permission to manage it.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Accommodation = accommodation;
            ViewBag.Units = accommodation.Units;

            return View();
        }

        // GET: CreateUnit
        public ActionResult CreateUnit(int accommodationId)
        {
            var accommodation = AccommodationService.GetAccommodationForEdit(accommodationId, CurrentUser.Username);

            if (accommodation == null)
            {
                TempData["Error"] = "Accommodation not found or you don't have permission to add units.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.AccommodationId = accommodationId;
            ViewBag.AccommodationName = accommodation.Name;

            return View();
        }

        // POST: CreateUnit
        [HttpPost]
        public ActionResult CreateUnit(int accommodationId, int maxGuests, bool petsAllowed, string price)
        {
            decimal priceValue;
            if (!decimal.TryParse(price.Replace(",", "."), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out priceValue))
            {
                ModelState.AddModelError("", "Invalid price format.");

                var accommodation = AccommodationService.GetAccommodationForEdit(accommodationId, CurrentUser.Username);
                ViewBag.AccommodationId = accommodationId;
                ViewBag.AccommodationName = accommodation?.Name ?? "Unknown";

                return View();
            }

            if (!AccommodationUnitService.CreateUnit(CurrentUser.Username, accommodationId, maxGuests,
                petsAllowed, priceValue, out string errorMessage))
            {
                ModelState.AddModelError("", errorMessage);

                var accommodation = AccommodationService.GetAccommodationForEdit(accommodationId, CurrentUser.Username);
                ViewBag.AccommodationId = accommodationId;
                ViewBag.AccommodationName = accommodation?.Name ?? "Unknown";

                return View();
            }

            TempData["Success"] = "Unit created successfully!";
            return RedirectToAction("ManageUnits", new { accommodationId });
        }

        // GET: EditUnit
        public ActionResult EditUnit(int id)
        {
            var unit = AccommodationUnitService.GetUnitForEdit(id, CurrentUser.Username);

            if (unit == null)
            {
                TempData["Error"] = "Unit not found or you don't have permission to edit it.";
                return RedirectToAction("Index", "Home");
            }

            var allGrouped = AccommodationUnitRepository.GetAllGrouped();
            int accommodationId = -1;

            foreach (var kvp in allGrouped)
            {
                if (kvp.Value.Any(u => u.Id == id))
                {
                    accommodationId = kvp.Key;
                    break;
                }
            }

            var accommodation = AccommodationRepository.GetById(accommodationId);
            ViewBag.AccommodationName = accommodation?.Name ?? "Unknown";
            ViewBag.AccommodationId = accommodationId;

            return View(unit);
        }

        // POST: EditUnit
        [HttpPost]
        public ActionResult EditUnit(int id, int maxGuests, bool petsAllowed, string price)
        {
            decimal priceValue;
            if (!decimal.TryParse(price.Replace(",", "."), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out priceValue))
            {
                ModelState.AddModelError("", "Invalid price format.");

                var unit = AccommodationUnitService.GetUnitForEdit(id, CurrentUser.Username);
                var allGrouped = AccommodationUnitRepository.GetAllGrouped();
                int accommodationId = -1;

                foreach (var kvp in allGrouped)
                {
                    if (kvp.Value.Any(u => u.Id == id))
                    {
                        accommodationId = kvp.Key;
                        break;
                    }
                }

                var accommodation = AccommodationRepository.GetById(accommodationId);
                ViewBag.AccommodationName = accommodation?.Name ?? "Unknown";
                ViewBag.AccommodationId = accommodationId;

                return View(unit);
            }

            if (!AccommodationUnitService.UpdateUnit(id, CurrentUser.Username, maxGuests, petsAllowed, priceValue, out string errorMessage))
            {
                ModelState.AddModelError("", errorMessage);

                var unit = AccommodationUnitService.GetUnitForEdit(id, CurrentUser.Username);
                var allGrouped = AccommodationUnitRepository.GetAllGrouped();
                int accommodationId = -1;

                foreach (var kvp in allGrouped)
                {
                    if (kvp.Value.Any(u => u.Id == id))
                    {
                        accommodationId = kvp.Key;
                        break;
                    }
                }

                var accommodation = AccommodationRepository.GetById(accommodationId);
                ViewBag.AccommodationName = accommodation?.Name ?? "Unknown";
                ViewBag.AccommodationId = accommodationId;

                return View(unit);
            }

            TempData["Success"] = "Unit updated successfully!";

            var allGrouped2 = AccommodationUnitRepository.GetAllGrouped();
            int parentAccommodationId = -1;

            foreach (var kvp in allGrouped2)
            {
                if (kvp.Value.Any(u => u.Id == id))
                {
                    parentAccommodationId = kvp.Key;
                    break;
                }
            }

            if (parentAccommodationId != -1)
                return RedirectToAction("ManageUnits", new { accommodationId = parentAccommodationId });
            else
                return RedirectToAction("Index", "Home");
        }

        // POST: DeleteUnit
        [HttpPost]
        public ActionResult DeleteUnit(int id)
        {
            var allGrouped = AccommodationUnitRepository.GetAllGrouped();
            int parentAccommodationId = -1;

            foreach (var kvp in allGrouped)
            {
                if (kvp.Value.Any(u => u.Id == id))
                {
                    parentAccommodationId = kvp.Key;
                    break;
                }
            }

            if (!AccommodationUnitService.DeleteUnit(id, CurrentUser.Username, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
            }
            else
            {
                TempData["Success"] = "Unit deleted successfully.";
            }

            if (parentAccommodationId != -1)
                return RedirectToAction("ManageUnits", new { accommodationId = parentAccommodationId });
            else
                return RedirectToAction("Index", "Home");
        }

        // GET: ViewAllUnits
        public ActionResult ViewAllUnits(int? minGuests = null, int? maxGuests = null, bool? petsAllowed = null,
            decimal? maxPrice = null, string unitSortBy = "", string unitSortDir = "asc")
        {
            var units = AccommodationUnitService.GetByManager(CurrentUser.Username);

            units = AccommodationUnitService.SearchUnits(units, minGuests, maxGuests, petsAllowed, maxPrice);

            switch (unitSortBy)
            {
                case "MaxGuests":
                    units = AccommodationUnitService.SortByMaxGuests(units, unitSortDir == "asc");
                    break;
                case "Price":
                    units = AccommodationUnitService.SortByPrice(units, unitSortDir == "asc");
                    break;
            }

            ViewBag.MyUnits = units;
            ViewBag.TotalUnits = units.Count;
            ViewBag.ActiveUnits = units.Count(u => !u.IsDeleted);
            ViewBag.DeletedUnits = units.Count(u => u.IsDeleted);

            ViewBag.MinGuests = minGuests;
            ViewBag.MaxGuests = maxGuests;
            ViewBag.PetsAllowed = petsAllowed;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = unitSortBy;
            ViewBag.SortDir = unitSortDir;

            return View();
        }

        #endregion


        #region Reservation Management

        // GET: ViewAllReservations
        public ActionResult ViewAllReservations()
        {
            var myArrangements = ArrangementService.GetByManager(CurrentUser.Username);
            var arrangementIds = myArrangements.Select(a => a.Id).ToList();

            var allReservations = ReservationRepository.GetAll();
            var myReservations = allReservations.Where(r => arrangementIds.Contains(r.SelectedArrangement.Id)).ToList();

            myReservations = myReservations.OrderByDescending(r => r.SelectedArrangement.StartDate).ToList();

            ViewBag.MyReservations = myReservations;
            ViewBag.TotalReservations = myReservations.Count;
            ViewBag.ActiveReservations = myReservations.Count(r => r.Status == ReservationStatusEnum.Active);
            ViewBag.CancelledReservations = myReservations.Count(r => r.Status == ReservationStatusEnum.Cancelled);

            return View();
        }

        // GET: ViewReservation (Manager version)
        public ActionResult ViewReservation(Guid reservationId)
        {
            var reservation = ReservationRepository.GetAll().FirstOrDefault(r => r.Id == reservationId);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("ViewAllReservations");
            }

            var myArrangements = ArrangementService.GetByManager(CurrentUser.Username);
            if (!myArrangements.Any(a => a.Id == reservation.SelectedArrangement.Id))
            {
                TempData["Error"] = "You can only view reservations for your own arrangements.";
                return RedirectToAction("ViewAllReservations");
            }

            return View(reservation);
        }

        #endregion

        #region Comment Management

        // GET: ApproveComments
        public ActionResult ApproveComments(string filter = "pending")
        {
            var comments = CommentRepository.GetByManagerUsername(CurrentUser.Username);

            switch (filter.ToLower())
            {
                case "pending":
                    comments = comments.Where(c => c.Status == CommentStatusEnum.Pending).ToList();
                    break;
                case "approved":
                    comments = comments.Where(c => c.Status == CommentStatusEnum.Approved).ToList();
                    break;
                case "rejected":
                    comments = comments.Where(c => c.Status == CommentStatusEnum.Rejected).ToList();
                    break;
                case "all":
                default:
                    // Show all
                    break;
            }

            ViewBag.Comments = comments;
            ViewBag.Filter = filter;
            ViewBag.PendingCount = CommentRepository.GetPendingByManagerUsername(CurrentUser.Username).Count;
            ViewBag.TotalCount = CommentRepository.GetByManagerUsername(CurrentUser.Username).Count;

            return View();
        }

        // POST: ApproveComment
        [HttpPost]
        public ActionResult ApproveComment(Guid commentId)
        {
            if (!CommentService.ApproveComment(commentId, CurrentUser.Username, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
            }
            else
            {
                TempData["Success"] = "Comment approved successfully!";
            }

            return RedirectToAction("ApproveComments");
        }

        // POST: RejectComment
        [HttpPost]
        public ActionResult RejectComment(Guid commentId)
        {
            if (!CommentService.RejectComment(commentId, CurrentUser.Username, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
            }
            else
            {
                TempData["Success"] = "Comment rejected successfully!";
            }

            return RedirectToAction("ApproveComments");
        }

        #endregion
    }
}