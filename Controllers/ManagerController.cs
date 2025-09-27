using System;
using System.Linq;
using System.Web.Mvc;
using Veb_Projekat.Filters;
using Veb_Projekat.Models.Enums;
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
    }
}