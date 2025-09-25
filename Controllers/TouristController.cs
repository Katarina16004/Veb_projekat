using System;
using System.Linq;
using System.Web.Mvc;
using Veb_Projekat.Filters;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;

namespace Veb_Projekat.Controllers
{
    [AuthorizeSession(RoleEnum.Tourist)]
    public class TouristController : BaseController
    {
        public ActionResult MyReservations(string search = "", string status = "", string sortBy = "", string sortDir = "asc")
        {
            var reservations = ReservationRepository.GetByTouristUsername(CurrentUser.Username);

            // Pretraga
            if (!string.IsNullOrEmpty(search))
            {
                reservations = reservations.Where(r =>
                    r.Id.ToString().Contains(search) ||
                    r.SelectedArrangement.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            // Pretraga po statusu
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(status, out ReservationStatusEnum statusEnum))
                {
                    reservations = reservations.Where(r => r.Status == statusEnum).ToList();
                }
            }

            // Sortiranje
            switch (sortBy)
            {
                case "ArrangementName":
                    reservations = sortDir == "asc"
                        ? reservations.OrderBy(r => r.SelectedArrangement.Name).ToList()
                        : reservations.OrderByDescending(r => r.SelectedArrangement.Name).ToList();
                    break;
            }

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
            var arrangement = ArrangementRepository.GetById(arrangementId);
            if (arrangement == null)
            {
                TempData["Error"] = "Arrangement not found.";
                return RedirectToAction("Index", "Home");
            }

            if (arrangement.StartDate <= DateTime.Now)
            {
                TempData["Error"] = "Cannot reserve past arrangements.";
                return RedirectToAction("Details", "Home", new { id = arrangementId });
            }

            AccommodationUnit unit = null;
            foreach (var acc in arrangement.Accommodations)
            {
                unit = acc.Units.FirstOrDefault(u => u.Id == unitId);
                if (unit != null) break;
            }

            if (unit == null)
            {
                TempData["Error"] = "Accommodation unit not found.";
                return RedirectToAction("Details", "Home", new { id = arrangementId });
            }

            var existingReservations = ReservationRepository.GetAll()
                .Where(r => r.SelectedUnit.Id == unitId && r.Status == ReservationStatusEnum.Active)
                .ToList();

            if (existingReservations.Any())
            {
                TempData["Error"] = "This accommodation unit is already booked.";
                return RedirectToAction("Details", "Home", new { id = arrangementId });
            }

            var tourist = UserRepository.GetByUsername(CurrentUser.Username);
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                Tourist = tourist,
                Status = ReservationStatusEnum.Active,
                SelectedArrangement = arrangement,
                SelectedUnit = unit
            };

            ReservationRepository.Add(reservation);

            TempData["Success"] = $"Successfully reserved {arrangement.Name}!";
            return RedirectToAction("MyReservations");
        }

        [HttpPost]
        public ActionResult CancelReservation(Guid reservationId)
        {
            var reservations = ReservationRepository.GetAll();
            var reservation = reservations.FirstOrDefault(r => r.Id == reservationId);

            if (reservation == null || reservation.Tourist.Username != CurrentUser.Username)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("MyReservations");
            }

            if (reservation.SelectedArrangement.StartDate <= DateTime.Now)
            {
                TempData["Error"] = "Cannot cancel reservation for past arrangements.";
                return RedirectToAction("MyReservations");
            }

            if (reservation.Status == ReservationStatusEnum.Cancelled)
            {
                TempData["Error"] = "This reservation is already cancelled.";
                return RedirectToAction("MyReservations");
            }

            ReservationRepository.UpdateStatus(reservationId, ReservationStatusEnum.Cancelled);

            TempData["Success"] = "Reservation cancelled successfully.";
            return RedirectToAction("MyReservations");
        }

        public ActionResult LeaveComment(int accommodationId, int reservationId)
        {
            var reservation = ReservationRepository.GetAll()
                .FirstOrDefault(r => r.Id.ToString() == reservationId.ToString() &&
                                     r.Tourist.Username == CurrentUser.Username);

            if (reservation == null)
            {
                TempData["Error"] = "Invalid reservation.";
                return RedirectToAction("MyReservations");
            }

            if (reservation.SelectedArrangement.EndDate > DateTime.Now)
            {
                TempData["Error"] = "You can only comment on completed trips.";
                return RedirectToAction("MyReservations");
            }

            var accommodation = reservation.SelectedArrangement.Accommodations
                .FirstOrDefault(a => a.Id == accommodationId);

            if (accommodation == null)
            {
                TempData["Error"] = "Accommodation not found.";
                return RedirectToAction("MyReservations");
            }

            ViewBag.Accommodation = accommodation;
            ViewBag.Reservation = reservation;
            return View();
        }

        [HttpPost]
        public ActionResult LeaveComment(int accommodationId, int reservationId, string commentText, int rating)
        {
            if (string.IsNullOrEmpty(commentText) || rating < 1 || rating > 5)
            {
                TempData["Error"] = "Please provide valid comment and rating (1-5).";
                return RedirectToAction("LeaveComment", new { accommodationId, reservationId });
            }

            var tourist = UserRepository.GetByUsername(CurrentUser.Username);
            var accommodation = AccommodationRepository.GetAllGrouped()
                .SelectMany(kvp => kvp.Value)
                .FirstOrDefault(a => a.Id == accommodationId);

            if (accommodation == null)
            {
                TempData["Error"] = "Accommodation not found.";
                return RedirectToAction("MyReservations");
            }

            var comment = new Comment
            {
                Tourist = tourist,
                Accommodation = accommodation,
                Text = commentText,
                Rating = rating
            };

            CommentRepository.Add(comment);

            TempData["Success"] = "Comment submitted successfully! It will be visible after manager approval.";
            return RedirectToAction("MyReservations");
        }

        public ActionResult ReservationDetails(Guid reservationId)
        {
            var reservation = ReservationRepository.GetAll()
                .FirstOrDefault(r => r.Id == reservationId && r.Tourist.Username == CurrentUser.Username);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("MyReservations");
            }

            return View(reservation);
        }
    }
}