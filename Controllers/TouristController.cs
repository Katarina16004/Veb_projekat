using System;
using System.Linq;
using System.Web.Mvc;
using Veb_Projekat.Filters;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Services;

namespace Veb_Projekat.Controllers
{
    [AuthorizeSession(RoleEnum.Tourist)]
    public class TouristController : BaseController
    {
        public ActionResult MyReservations(string search = "", string status = "", string sortBy = "", string sortDir = "asc")
        {
            var reservations = ReservationService.SearchReservations(CurrentUser.Username, search, status, sortBy, sortDir);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.Reservations = reservations;

            return View();
        }

        [HttpPost]
        public ActionResult Reserve(int arrangementId, int unitId)
        {
            if (!ReservationService.CanReserveUnit(arrangementId, unitId, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction("Details", "Home", new { id = arrangementId });
            }

            var reservation = ReservationService.CreateReservation(CurrentUser.Username, arrangementId, unitId);

            TempData["Success"] = $"Successfully reserved {reservation.SelectedArrangement.Name}!";
            return RedirectToAction("MyReservations");
        }

        [HttpPost]
        public ActionResult CancelReservation(Guid reservationId)
        {
            if (!ReservationService.CanCancelReservation(reservationId, CurrentUser.Username, out Reservation reservation, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction("MyReservations");
            }

            ReservationService.CancelReservation(reservationId);

            TempData["Success"] = "Reservation cancelled successfully.";
            return RedirectToAction("MyReservations");
        }

        public ActionResult LeaveComment(int accommodationId, string reservationId)
        {
            if (!CommentService.CanLeaveComment(accommodationId, reservationId, CurrentUser.Username, out Reservation reservation, out Accommodation accommodation, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction("MyReservations");
            }

            ViewBag.Accommodation = accommodation;
            ViewBag.Reservation = reservation;
            return View();
        }

        [HttpPost]
        public ActionResult LeaveComment(int accommodationId, string reservationId, string commentText, int rating)
        {
            if (!CommentService.CreateComment(accommodationId, CurrentUser.Username, commentText, rating, out string errorMessage))
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction("LeaveComment", new { accommodationId, reservationId });
            }

            TempData["Success"] = "Comment submitted successfully! It will be visible after manager approval.";
            return RedirectToAction("MyReservations");
        }

        public ActionResult ReservationDetails(Guid reservationId)
        {
            var reservation = ReservationService.GetReservationDetails(reservationId, CurrentUser.Username);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("MyReservations");
            }

            return View(reservation);
        }
    }
}