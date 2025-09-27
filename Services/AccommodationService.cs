using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;

namespace Veb_Projekat.Services
{
    public class AccommodationService
    {
        public static List<Accommodation> SearchAccommodations(List<Accommodation> accommodations, string type = "", string name = "", bool? hasPool = null,
            bool? hasSpa = null, bool? accessible = null, bool? hasWifi = null)
        {
            var result = new List<Accommodation>();

            foreach (var acc in accommodations)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(type) && !acc.Type.ToString().Equals(type, StringComparison.OrdinalIgnoreCase))
                    match = false;

                if (!string.IsNullOrEmpty(name) && acc.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0)
                    match = false;

                if (hasPool.HasValue && acc.HasPool != hasPool.Value)
                    match = false;

                if (hasSpa.HasValue && acc.HasSpa != hasSpa.Value)
                    match = false;

                if (accessible.HasValue && acc.Accessible != accessible.Value)
                    match = false;

                if (hasWifi.HasValue && acc.HasWifi != hasWifi.Value)
                    match = false;

                if (match)
                    result.Add(acc);
            }

            return result;
        }

        public static List<Accommodation> SortByName(List<Accommodation> accommodations, bool ascending = true)
        {
            return ascending
                ? accommodations.OrderBy(a => a.Name).ToList()
                : accommodations.OrderByDescending(a => a.Name).ToList();
        }

        public static List<Accommodation> SortByTotalUnits(List<Accommodation> accommodations, bool ascending = true)
        {
            return ascending
                ? accommodations.OrderBy(a => a.Units.Count).ToList()
                : accommodations.OrderByDescending(a => a.Units.Count).ToList();
        }

        public static List<Accommodation> SortByAvailableUnits(List<Accommodation> accommodations, bool ascending = true)
        {
            return ascending
                ? accommodations.OrderBy(a => GetAvailableUnitsCount(a)).ToList()
                : accommodations.OrderByDescending(a => GetAvailableUnitsCount(a)).ToList();
        }

        private static int GetAvailableUnitsCount(Accommodation acc)
        {
            var reservations = ReservationRepository.GetAll();
            return acc.Units.Count(u =>
                !reservations.Any(r => r.SelectedUnit.Id == u.Id &&
                                       r.Status == ReservationStatusEnum.Active));
        }



        public static List<Accommodation> GetByManager(string managerUsername)
        {
            return AccommodationRepository.GetByManagerUsername(managerUsername);
        }

        public static Accommodation GetAccommodationForEdit(int id, string managerUsername)
        {
            var accommodation = AccommodationRepository.GetById(id);

            if (accommodation == null)
                return null;

            if (!CanManageAccommodation(accommodation, managerUsername))
                return null;

            return accommodation;
        }

        public static bool CanManageAccommodation(Accommodation accommodation, string managerUsername)
        {
            var arrangements = ArrangementRepository.GetAllByManagerUsername(managerUsername);
            return arrangements.Any(arr => arr.Accommodations.Any(acc => acc.Id == accommodation.Id));
        }

        public static bool ValidateAccommodationData(string name, int stars, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "Accommodation name is required.";
                return false;
            }

            if (stars < 0 || stars > 5)
            {
                errorMessage = "Stars must be between 0 and 5.";
                return false;
            }

            return true;
        }

        public static bool CreateAccommodation(string managerUsername, int arrangementId, string name, AccommodationTypeEnum type,
            int stars, bool hasPool, bool hasSpa, bool accessible, bool hasWifi, out string errorMessage)
        {
            errorMessage = "";

            var arrangement = ArrangementRepository.GetById(arrangementId);
            if (arrangement == null || arrangement.ManagerUsername != managerUsername)
            {
                errorMessage = "You can only add accommodations to your own arrangements.";
                return false;
            }

            if (!ValidateAccommodationData(name, stars, out errorMessage))
            {
                return false;
            }

            var accommodation = new Accommodation
            {
                Id = AccommodationRepository.GetNextId(),
                Name = name,
                Type = type,
                Stars = stars,
                HasPool = hasPool,
                HasSpa = hasSpa,
                Accessible = accessible,
                HasWifi = hasWifi,
                Units = new List<AccommodationUnit>(),
                IsDeleted = false
            };

            try
            {
                AccommodationRepository.Add(accommodation, arrangementId);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error creating accommodation: " + ex.Message;
                return false;
            }
        }

        public static bool UpdateAccommodation(int id, string managerUsername, string name, AccommodationTypeEnum type,
            int stars, bool hasPool, bool hasSpa, bool accessible, bool hasWifi, out string errorMessage)
        {
            errorMessage = "";

            var accommodation = AccommodationRepository.GetById(id);
            if (accommodation == null)
            {
                errorMessage = "Accommodation not found.";
                return false;
            }

            if (!CanManageAccommodation(accommodation, managerUsername))
            {
                errorMessage = "You can only edit your own accommodations.";
                return false;
            }

            if (accommodation.IsDeleted)
            {
                errorMessage = "Cannot edit deleted accommodation.";
                return false;
            }

            if (!ValidateAccommodationData(name, stars, out errorMessage))
            {
                return false;
            }

            try
            {
                AccommodationRepository.Update(id, name, type, stars, hasPool, hasSpa, accessible, hasWifi);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error updating accommodation: " + ex.Message;
                return false;
            }
        }

        public static bool CanDeleteAccommodation(int accommodationId, string managerUsername, out string errorMessage)
        {
            errorMessage = "";

            var accommodation = AccommodationRepository.GetById(accommodationId);
            if (accommodation == null)
            {
                errorMessage = "Accommodation not found.";
                return false;
            }

            if (!CanManageAccommodation(accommodation, managerUsername))
            {
                errorMessage = "You can only delete your own accommodations.";
                return false;
            }

            if (accommodation.IsDeleted)
            {
                errorMessage = "Accommodation is already deleted.";
                return false;
            }

            // da li postoje buduci aranzmani
            var futureArrangements = ArrangementRepository.GetAll().Where(arr => arr.StartDate > DateTime.Now &&
                             !arr.IsDeleted && arr.Accommodations.Any(acc => acc.Id == accommodationId))
                .ToList();

            if (futureArrangements.Any())
            {
                errorMessage = "Cannot delete accommodation used in future arrangements.";
                return false;
            }

            return true;
        }

        public static bool DeleteAccommodation(int accommodationId, string managerUsername, out string errorMessage)
        {
            errorMessage = "";

            if (!CanDeleteAccommodation(accommodationId, managerUsername, out errorMessage))
            {
                return false;
            }

            try
            {
                AccommodationRepository.LogicalDelete(accommodationId);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error deleting accommodation: " + ex.Message;
                return false;
            }
        }
    }
}