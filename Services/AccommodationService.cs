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
    }
}