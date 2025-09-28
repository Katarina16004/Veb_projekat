using System;
using System.Collections.Generic;
using System.Linq;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;

namespace Veb_Projekat.Services
{
    public class AccommodationUnitService
    {
        public static List<AccommodationUnit> SearchUnits(List<AccommodationUnit> units, int? minGuests = null, int? maxGuests = null,
            bool? petsAllowed = null, decimal? maxPrice = null)
        {
            return units.Where(u =>
                (!minGuests.HasValue || u.MaxGuests >= minGuests.Value) &&
                (!maxGuests.HasValue || u.MaxGuests <= maxGuests.Value) &&
                (!petsAllowed.HasValue || u.PetsAllowed == petsAllowed.Value) &&
                (!maxPrice.HasValue || u.Price <= maxPrice.Value)
            ).ToList();
        }

        public static List<AccommodationUnit> SortByMaxGuests(List<AccommodationUnit> units, bool ascending = true)
        {
            return ascending
                ? units.OrderBy(u => u.MaxGuests).ToList()
                : units.OrderByDescending(u => u.MaxGuests).ToList();
        }

        public static List<AccommodationUnit> SortByPrice(List<AccommodationUnit> units, bool ascending = true)
        {
            return ascending
                ? units.OrderBy(u => u.Price).ToList()
                : units.OrderByDescending(u => u.Price).ToList();
        }



        public static List<AccommodationUnit> GetByManager(string managerUsername)
        {
            var accommodations = AccommodationService.GetByManager(managerUsername);
            var units = new List<AccommodationUnit>();

            foreach (var accommodation in accommodations)
            {
                units.AddRange(accommodation.Units);
            }

            return units.GroupBy(u => u.Id).Select(g => g.First()).ToList(); 
        }

        public static AccommodationUnit GetUnitForEdit(int unitId, string managerUsername)
        {
            var unit = AccommodationUnitRepository.GetById(unitId);
            if (unit == null)
                return null;

            if (!CanManageUnit(unitId, managerUsername))
                return null;

            return unit;
        }

        public static bool CanManageUnit(int unitId, string managerUsername)
        {
            var allGrouped = AccommodationUnitRepository.GetAllGrouped();
            int accommodationId = -1;

            foreach (var kvp in allGrouped)
            {
                if (kvp.Value.Any(u => u.Id == unitId))
                {
                    accommodationId = kvp.Key;
                    break;
                }
            }

            if (accommodationId == -1) return false;

            
            var accommodation = AccommodationRepository.GetById(accommodationId);
            return AccommodationService.CanManageAccommodation(accommodation, managerUsername);
        }

        public static bool ValidateUnitData(int maxGuests, decimal price, out string errorMessage)
        {
            errorMessage = "";

            if (maxGuests <= 0)
            {
                errorMessage = "Max guests must be greater than 0.";
                return false;
            }

            if (maxGuests > 20)
            {
                errorMessage = "Max guests cannot exceed 20.";
                return false;
            }

            if (price <= 0)
            {
                errorMessage = "Price must be greater than 0.";
                return false;
            }

            if (price > 10000)
            {
                errorMessage = "Price cannot exceed 10,000.";
                return false;
            }

            return true;
        }

        public static bool CreateUnit(string managerUsername, int accommodationId, int maxGuests,
            bool petsAllowed, decimal price, out string errorMessage)
        {
            errorMessage = "";

           
            var accommodation = AccommodationRepository.GetById(accommodationId);
            if (accommodation == null || !AccommodationService.CanManageAccommodation(accommodation, managerUsername))
            {
                errorMessage = "You can only add units to your own accommodations.";
                return false;
            }

            if (!ValidateUnitData(maxGuests, price, out errorMessage))
            {
                return false;
            }

            var unit = new AccommodationUnit
            {
                Id = AccommodationUnitRepository.GetNextId(),
                MaxGuests = maxGuests,
                PetsAllowed = petsAllowed,
                Price = price,
                IsDeleted = false
            };

            try
            {
                AccommodationUnitRepository.Add(unit, accommodationId);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error creating unit: " + ex.Message;
                return false;
            }
        }

        public static bool CanEditUnit(int unitId, string managerUsername, int newMaxGuests, out string errorMessage)
        {
            errorMessage = "";

            var unit = AccommodationUnitRepository.GetById(unitId);
            if (unit == null)
            {
                errorMessage = "Unit not found.";
                return false;
            }

            if (!CanManageUnit(unitId, managerUsername))
            {
                errorMessage = "You can only edit your own units.";
                return false;
            }

            if (unit.IsDeleted)
            {
                errorMessage = "Cannot edit deleted unit.";
                return false;
            }

            // da li postoje aktivne rezervacije za buduce aranzmane
            if (unit.MaxGuests != newMaxGuests && HasFutureActiveReservations(unitId))
            {
                errorMessage = "Cannot modify number of beds - unit has active reservations for future arrangements.";
                return false;
            }

            return true;
        }

        public static bool UpdateUnit(int unitId, string managerUsername, int maxGuests,
            bool petsAllowed, decimal price, out string errorMessage)
        {
            errorMessage = "";

            if (!CanEditUnit(unitId, managerUsername, maxGuests, out errorMessage))
            {
                return false;
            }

            if (!ValidateUnitData(maxGuests, price, out errorMessage))
            {
                return false;
            }

            try
            {
                AccommodationUnitRepository.Update(unitId, maxGuests, petsAllowed, price);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error updating unit: " + ex.Message;
                return false;
            }
        }

        public static bool CanDeleteUnit(int unitId, string managerUsername, out string errorMessage)
        {
            errorMessage = "";

            var unit = AccommodationUnitRepository.GetById(unitId);
            if (unit == null)
            {
                errorMessage = "Unit not found.";
                return false;
            }

            if (!CanManageUnit(unitId, managerUsername))
            {
                errorMessage = "You can only delete your own units.";
                return false;
            }

            if (unit.IsDeleted)
            {
                errorMessage = "Unit is already deleted.";
                return false;
            }

            // da li postoje aktivne rezervacije za buduce aranzmane
            if (HasFutureActiveReservations(unitId))
            {
                errorMessage = "Cannot delete unit - it has active reservations for future arrangements.";
                return false;
            }

            return true;
        }

        public static bool DeleteUnit(int unitId, string managerUsername, out string errorMessage)
        {
            errorMessage = "";

            if (!CanDeleteUnit(unitId, managerUsername, out errorMessage))
            {
                return false;
            }

            try
            {
                AccommodationUnitRepository.LogicalDelete(unitId);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error deleting unit: " + ex.Message;
                return false;
            }
        }

        private static bool HasFutureActiveReservations(int unitId)
        {
            var reservations = ReservationRepository.GetAll();

            return reservations.Any(r =>
                r.SelectedUnit.Id == unitId &&
                r.Status == ReservationStatusEnum.Active &&
                r.SelectedArrangement.StartDate > DateTime.Now
            );
        }
    }
}