using System;
using System.Linq;
using System.Web.Mvc;
using Veb_Projekat.Filters;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;
using Veb_Projekat.Services;

namespace Veb_Projekat.Controllers
{
    [AuthorizeSession(RoleEnum.Manager)]
    public class ManagerController : BaseController
    {
        public ActionResult Index()
        {
            var myArrangements = ArrangementService.GetByManager(CurrentUser.Username);

            ViewBag.MyArrangements = myArrangements;
            ViewBag.TotalArrangements = myArrangements.Count;
            ViewBag.UpcomingArrangements = myArrangements.Count(a => a.StartDate > DateTime.Now);
            ViewBag.PastArrangements = myArrangements.Count(a => a.StartDate <= DateTime.Now);

            return View();
        }

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
            return RedirectToAction("Index");
        }

        // GET: EditArrangement
        public ActionResult EditArrangement(int id)
        {
            var arrangement = ArrangementService.GetArrangementForEdit(id, CurrentUser.Username);

            if (arrangement == null)
            {
                TempData["Error"] = "Arrangement not found or you don't have permission to edit it.";
                return RedirectToAction("Index");
            }

            if (!ArrangementService.CanEditArrangement(id, CurrentUser.Username, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction("Index");
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
            return RedirectToAction("Index");
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

            return RedirectToAction("Index");
        }

        // GET: ViewAllArrangements
        public ActionResult ViewAllArrangements(bool includeDeleted = false)
        {
            var arrangements = ArrangementService.GetAllByManager(CurrentUser.Username, includeDeleted);

            ViewBag.AllArrangements = arrangements;
            ViewBag.IncludeDeleted = includeDeleted;
            ViewBag.ActiveCount = arrangements.Count(a => !a.IsDeleted);
            ViewBag.DeletedCount = arrangements.Count(a => a.IsDeleted);

            return View();
        }

        public ActionResult ApproveComments()
        {
            return View();
        }


        // GET: ManageAccommodations
        public ActionResult ManageAccommodations(int arrangementId)
        {
            var arrangement = ArrangementService.GetArrangementForEdit(arrangementId, CurrentUser.Username);

            if (arrangement == null)
            {
                TempData["Error"] = "Arrangement not found or you don't have permission to manage it.";
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
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

            var updatedAccommodation = AccommodationRepository.GetById(id);
            var arrangements = ArrangementRepository.GetAllByManagerUsername(CurrentUser.Username);
            var parentArrangement = arrangements.FirstOrDefault(arr => arr.Accommodations.Any(acc => acc.Id == id));

            if (parentArrangement != null)
                return RedirectToAction("ManageAccommodations", new { arrangementId = parentArrangement.Id });
            else
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
        }

        // GET: ViewAllAccommodations
        public ActionResult ViewAllAccommodations()
        {
            var accommodations = AccommodationService.GetByManager(CurrentUser.Username);

            ViewBag.MyAccommodations = accommodations;
            ViewBag.TotalAccommodations = accommodations.Count;
            ViewBag.ActiveAccommodations = accommodations.Count(a => !a.IsDeleted);
            ViewBag.DeletedAccommodations = accommodations.Count(a => a.IsDeleted);

            return View();
        }
    }
}