using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;

namespace Veb_Projekat.Repositories
{
    public class AccommodationUnitRepository
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Data/AccommodationUnits.txt");

        public static Dictionary<int, List<AccommodationUnit>> GetAllGrouped()
        {
            var unitsByAccommodation = new Dictionary<int, List<AccommodationUnit>>();

            if (!File.Exists(filePath))
                return unitsByAccommodation;

            var lines = File.ReadAllLines(filePath).Skip(1);

            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length < 5)
                    continue;

                int id = int.Parse(parts[0]);
                int accommodationId = int.Parse(parts[1]);
                int maxGuests = int.Parse(parts[2]);
                bool petsAllowed = bool.Parse(parts[3]);
                decimal price = decimal.Parse(parts[4]);

                var unit = new AccommodationUnit
                {
                    Id = id,
                    MaxGuests = maxGuests,
                    PetsAllowed = petsAllowed,
                    Price = price
                };

                if (!unitsByAccommodation.ContainsKey(accommodationId))
                    unitsByAccommodation[accommodationId] = new List<AccommodationUnit>();

                unitsByAccommodation[accommodationId].Add(unit);
            }

            return unitsByAccommodation;
        }

        public static List<AccommodationUnit> GetByAccommodationId(int accommodationId)
        {
            var grouped = GetAllGrouped();
            if (grouped.ContainsKey(accommodationId))
                return grouped[accommodationId];
            else
                return new List<AccommodationUnit>();
        }

        public static void Add(AccommodationUnit unit, int accommodationId)
        {
            bool fileExists = File.Exists(filePath);

            using (var sw = new StreamWriter(filePath, true))
            {
                if (!fileExists)
                {
                    sw.WriteLine("ID;AccommodationID;MaxGuests;PetsAllowed;Price;");
                }

                string line = $"{unit.Id};{accommodationId};{unit.MaxGuests};{unit.PetsAllowed};{unit.Price}";
                sw.WriteLine(line);
            }
        }
    }
}