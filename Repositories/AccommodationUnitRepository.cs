using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;

namespace Veb_Projekat.Repositories
{
    public class AccommodationUnitRepository
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Data/AccommodationUnits.txt");

        public static List<AccommodationUnit> GetAll()
        {
            var units = new List<AccommodationUnit>();

            if (!File.Exists(filePath))
                return units;

            var lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length < 5)
                    continue;

                bool isDeleted = false;
                bool.TryParse(parts[5], out isDeleted);

                var unit = new AccommodationUnit
                {
                    Id = int.Parse(parts[0]),
                    MaxGuests = int.Parse(parts[2]),
                    PetsAllowed = bool.Parse(parts[3]),
                    Price = decimal.Parse(parts[4], CultureInfo.InvariantCulture),
                    IsDeleted = isDeleted
                };

                units.Add(unit);
            }

            return units;
        }

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
                decimal price = decimal.Parse(parts[4], CultureInfo.InvariantCulture);

                bool isDeleted = false;
                if (parts.Length > 5)
                    bool.TryParse(parts[5], out isDeleted);

                if (isDeleted) continue;

                var unit = new AccommodationUnit
                {
                    Id = id,
                    MaxGuests = maxGuests,
                    PetsAllowed = petsAllowed,
                    Price = price,
                    IsDeleted = isDeleted
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

        public static AccommodationUnit GetById(int id)
        {
            return GetAll().FirstOrDefault(u => u.Id == id && !u.IsDeleted);
        }

        public static int GetNextId()
        {
            var units = GetAll();
            return units.Any() ? units.Max(u => u.Id) + 1 : 1;
        }

        public static void Add(AccommodationUnit unit, int accommodationId)
        {
            bool fileExists = File.Exists(filePath);

            using (var sw = new StreamWriter(filePath, true))
            {
                if (!fileExists)
                {
                    sw.WriteLine("ID;AccommodationID;MaxGuests;PetsAllowed;Price;IsDeleted");
                }

                string line = $"{unit.Id};{accommodationId};{unit.MaxGuests};" +
                             $"{unit.PetsAllowed.ToString().ToLower()};" +
                             $"{unit.Price.ToString(CultureInfo.InvariantCulture)};" +
                             $"{unit.IsDeleted.ToString().ToLower()}";

                sw.WriteLine(line);
            }
        }

        public static void Update(int id, int maxGuests, bool petsAllowed, decimal price)
        {
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length >= 5 && int.Parse(parts[0]) == id)
                {
                    string originalAccommodationId = parts[1];
                    string isDeleted = parts.Length > 5 ? parts[5] : "false";

                    lines[i] = $"{id};{originalAccommodationId};{maxGuests};" +
                              $"{petsAllowed.ToString().ToLower()};" +
                              $"{price.ToString(CultureInfo.InvariantCulture)};" +
                              $"{isDeleted}";
                    break;
                }
            }

            File.WriteAllLines(filePath, lines);
        }

        public static void LogicalDelete(int id)
        {
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length >= 5 && int.Parse(parts[0]) == id)
                {
                       
                    parts[5] = "true";
                    lines[i] = string.Join(";", parts);

                    File.WriteAllLines(filePath, lines);
                    break;
                }
            }
        }
    }
}