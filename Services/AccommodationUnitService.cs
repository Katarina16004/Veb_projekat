using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    }
}