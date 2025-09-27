using System;
using System.Collections.Generic;
using System.Linq;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;

namespace Veb_Projekat.Services
{
    public class ArrangementService
    {
        public static List<Arrangement> SearchArrangements(string name = "", string location = "", string type = "", string transport = "", DateTime? startDateFrom = null,
                 DateTime? startDateTo = null, DateTime? endDateFrom = null, DateTime? endDateTo = null)
        {
            // samo aktivni
            var all = ArrangementRepository.GetActiveOnly();
            var result = new List<Arrangement>();

            foreach (var arr in all)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(name) && arr.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0)
                    match = false;

                if (!string.IsNullOrEmpty(location) && arr.Location.IndexOf(location, StringComparison.OrdinalIgnoreCase) < 0)
                    match = false;

                if (!string.IsNullOrEmpty(type) && !arr.Type.ToString().Equals(type))
                    match = false;

                if (!string.IsNullOrEmpty(transport) && !arr.Transport.ToString().Equals(transport))
                    match = false;

                if (startDateFrom.HasValue && arr.StartDate < startDateFrom.Value)
                    match = false;

                if (startDateTo.HasValue && arr.StartDate > startDateTo.Value)
                    match = false;

                if (endDateFrom.HasValue && arr.EndDate < endDateFrom.Value)
                    match = false;

                if (endDateTo.HasValue && arr.EndDate > endDateTo.Value)
                    match = false;

                if (match)
                    result.Add(arr);
            }

            return result;
        }

        public static Arrangement GetDetails(int id)
        {
            return ArrangementRepository.GetById(id);
        }

        public static List<Arrangement> SortByName(List<Arrangement> arrangements, bool ascending = true)
        {
            if (ascending)
                return arrangements.OrderBy(a => a.Name).ToList();
            else
                return arrangements.OrderByDescending(a => a.Name).ToList();
        }

        public static List<Arrangement> SortByStartDate(List<Arrangement> arrangements, bool ascending = true)
        {
            if (ascending)
                return arrangements.OrderBy(a => a.StartDate).ToList();
            else
                return arrangements.OrderByDescending(a => a.StartDate).ToList();
        }

        public static List<Arrangement> SortByEndDate(List<Arrangement> arrangements, bool ascending = true)
        {
            if (ascending)
                return arrangements.OrderBy(a => a.EndDate).ToList();
            else
                return arrangements.OrderByDescending(a => a.EndDate).ToList();
        }


        public static List<Arrangement> GetByManager(string managerUsername)
        {
            return ArrangementRepository.GetActiveByManagerUsername(managerUsername);
        }

        public static List<Arrangement> GetAllByManager(string managerUsername, bool includeDeleted = false)
        {
            return includeDeleted
                ? ArrangementRepository.GetAllByManagerUsername(managerUsername)
                : ArrangementRepository.GetActiveByManagerUsername(managerUsername);
        }

        public static int GetNextId()
        {
            return ArrangementRepository.GetNextId();
        }

        public static bool ValidateArrangementData(string name, DateTime startDate, DateTime endDate, int maxPassengers, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "Arrangement name is required.";
                return false;
            }

            if (startDate <= DateTime.Now)
            {
                errorMessage = "Start date must be in the future.";
                return false;
            }

            if (endDate <= startDate)
            {
                errorMessage = "End date must be after start date.";
                return false;
            }

            if (maxPassengers <= 0)
            {
                errorMessage = "Max passengers must be greater than 0.";
                return false;
            }

            return true;
        }

        public static bool CreateArrangement(string managerUsername, string name, ArrangementTypeEnum type,
            TransportTypeEnum transport, string location, DateTime startDate, DateTime endDate,
            int maxPassengers, string description, string program, out string errorMessage)
        {
            errorMessage = "";

            if (!ValidateArrangementData(name, startDate, endDate, maxPassengers, out errorMessage))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                errorMessage = "Location is required.";
                return false;
            }

            var arrangement = new Arrangement
            {
                Id = GetNextId(),
                Name = name,
                Type = type,
                Transport = transport,
                Location = location,
                StartDate = startDate,
                EndDate = endDate,
                MaxNumOfPassengers = maxPassengers,
                Description = description ?? "",
                TravelProgram = program ?? "",
                Poster = name.ToLower().Replace(" ", "_") + ".png",
                ManagerUsername = managerUsername,
                Accommodations = new List<Accommodation>(),
                IsDeleted = false
            };

            try
            {
                ArrangementRepository.Add(arrangement);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error creating arrangement: " + ex.Message;
                return false;
            }
        }

        public static Arrangement GetArrangementForEdit(int id, string managerUsername)
        {
            var arrangement = ArrangementRepository.GetById(id);

            if (arrangement == null || arrangement.ManagerUsername != managerUsername)
                return null;

            return arrangement;
        }

        public static bool CanEditArrangement(int arrangementId, string managerUsername, out string errorMessage)
        {
            errorMessage = "";

            var arrangement = ArrangementRepository.GetById(arrangementId);
            if (arrangement == null)
            {
                errorMessage = "Arrangement not found.";
                return false;
            }

            if (arrangement.ManagerUsername != managerUsername)
            {
                errorMessage = "You can only edit your own arrangements.";
                return false;
            }

            if (arrangement.IsDeleted)
            {
                errorMessage = "Cannot edit deleted arrangement.";
                return false;
            }
            return true;
        }

        public static bool UpdateArrangement(int id, string managerUsername, string name, ArrangementTypeEnum type,
            TransportTypeEnum transport, string location, DateTime startDate, DateTime endDate,
            int maxPassengers, string description, string program, out string errorMessage)
        {
            errorMessage = "";

            if (!CanEditArrangement(id, managerUsername, out errorMessage))
            {
                return false;
            }

            if (!ValidateArrangementData(name, startDate, endDate, maxPassengers, out errorMessage))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                errorMessage = "Location is required.";
                return false;
            }

            try
            {
                ArrangementRepository.Update(id, name, type, transport, location, startDate, endDate,
                    maxPassengers, description, program);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error updating arrangement: " + ex.Message;
                return false;
            }
        }

        public static bool CanDeleteArrangement(int arrangementId, string managerUsername, out string errorMessage)
        {
            errorMessage = "";

            var arrangement = ArrangementRepository.GetById(arrangementId);
            if (arrangement == null)
            {
                errorMessage = "Arrangement not found.";
                return false;
            }

            if (arrangement.ManagerUsername != managerUsername)
            {
                errorMessage = "You can only delete your own arrangements.";
                return false;
            }

            if (arrangement.IsDeleted)
            {
                errorMessage = "Arrangement is already deleted.";
                return false;
            }

            // da li postoje rezervacije
            var reservations = ReservationRepository.GetAll().Where(r => r.SelectedArrangement.Id == arrangementId).ToList();

            if (reservations.Any())
            {
                errorMessage = "Cannot delete arrangement with existing reservations.";
                return false;
            }

            return true;
        }

        public static bool DeleteArrangement(int arrangementId, string managerUsername, out string errorMessage)
        {
            errorMessage = "";

            if (!CanDeleteArrangement(arrangementId, managerUsername, out errorMessage))
            {
                return false;
            }

            try
            {
                ArrangementRepository.LogicalDelete(arrangementId);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error deleting arrangement: " + ex.Message;
                return false;
            }
        }
    }
}