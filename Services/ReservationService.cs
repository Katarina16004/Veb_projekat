using System;
using System.Collections.Generic;
using System.Linq;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;

namespace Veb_Projekat.Services
{
    public class ReservationService
    {
        public static List<Reservation> SearchReservations(string username, string search = "", string status = "", string sortBy = "", string sortDir = "asc")
        {
            var reservations = ReservationRepository.GetByTouristUsername(username);

            // Pretraga
            if (!string.IsNullOrEmpty(search))
            {
                reservations = reservations.Where(r =>
                    r.Id.ToString().Contains(search) ||
                    r.SelectedArrangement.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            // Filter po statusu
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(status, out ReservationStatusEnum statusEnum))
                {
                    reservations = reservations.Where(r => r.Status == statusEnum).ToList();
                }
            }
            // Sortiranje
            if (sortBy == "ArrangementName")
            {
                reservations = sortDir == "asc"
                    ? reservations.OrderBy(r => r.SelectedArrangement.Name).ToList()
                    : reservations.OrderByDescending(r => r.SelectedArrangement.Name).ToList();
            }

            return reservations;
        }

        public static bool CanReserveUnit(int arrangementId, int unitId, out string errorMessage)
        {
            errorMessage = "";

            var arrangement = ArrangementRepository.GetById(arrangementId);
            if (arrangement == null)
            {
                errorMessage = "Arrangement not found.";
                return false;
            }

            if (arrangement.StartDate <= DateTime.Now)
            {
                errorMessage = "Cannot reserve past arrangements.";
                return false;
            }

            AccommodationUnit unit = null;
            foreach (var acc in arrangement.Accommodations)
            {
                unit = acc.Units.FirstOrDefault(u => u.Id == unitId);
                if (unit != null)   
                    break;
            }

            if (unit == null)
            {
                errorMessage = "Accommodation unit not found.";
                return false;
            }

            var existingReservations = ReservationRepository.GetAll().Where(r => r.SelectedUnit.Id == unitId && r.Status == ReservationStatusEnum.Active).ToList();

            if (existingReservations.Any())
            {
                errorMessage = "This accommodation unit is already booked.";
                return false;
            }

            return true;
        }

        public static Reservation CreateReservation(string touristUsername, int arrangementId, int unitId)
        {
            var arrangement = ArrangementRepository.GetById(arrangementId);
            var tourist = UserRepository.GetByUsername(touristUsername);

            AccommodationUnit unit = null;
            foreach (var acc in arrangement.Accommodations)
            {
                unit = acc.Units.FirstOrDefault(u => u.Id == unitId);
                if (unit != null) 
                    break;
            }

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                Tourist = tourist,
                Status = ReservationStatusEnum.Active,
                SelectedArrangement = arrangement,
                SelectedUnit = unit
            };

            ReservationRepository.Add(reservation);
            return reservation;
        }

        public static bool CanCancelReservation(Guid reservationId, string touristUsername, out Reservation reservation, out string errorMessage)
        {
            errorMessage = "";

            var reservations = ReservationRepository.GetAll();
            reservation = reservations.FirstOrDefault(r => r.Id == reservationId);

            if (reservation == null || reservation.Tourist.Username != touristUsername)
            {
                errorMessage = "Reservation not found.";
                return false;
            }

            if (reservation.SelectedArrangement.StartDate <= DateTime.Now)
            {
                errorMessage = "Cannot cancel reservation for past arrangements.";
                return false;
            }

            if (reservation.Status == ReservationStatusEnum.Cancelled)
            {
                errorMessage = "This reservation is already cancelled.";
                return false;
            }

            return true;
        }

        public static void CancelReservation(Guid reservationId)
        {
            ReservationRepository.UpdateStatus(reservationId, ReservationStatusEnum.Cancelled);
        }

        public static Reservation GetReservationDetails(Guid reservationId, string touristUsername)
        {
            return ReservationRepository.GetAll()
                .FirstOrDefault(r => r.Id == reservationId && r.Tourist.Username == touristUsername);
        }

        public static bool IsUnitBooked(int unitId)
        {
            var reservations = ReservationRepository.GetAll();
            return reservations.Any(r => r.SelectedUnit.Id == unitId && r.Status == ReservationStatusEnum.Active);
        }
    }
}