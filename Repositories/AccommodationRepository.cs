using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Repositories
{
    public class AccommodationRepository
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Data/Accommodations.txt");

        public static List<Accommodation> GetAll()
        {
            var accommodations = new List<Accommodation>();

            if (!File.Exists(filePath))
                return accommodations;

            var lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length < 9) 
                    continue;

                bool isDeleted = false;
                bool.TryParse(parts[9], out isDeleted);

                var accommodation = new Accommodation
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1],
                    Type = (AccommodationTypeEnum)Enum.Parse(typeof(AccommodationTypeEnum), parts[2], true),
                    Stars = int.Parse(parts[3]),
                    HasPool = bool.Parse(parts[4]),
                    HasSpa = bool.Parse(parts[5]),
                    Accessible = bool.Parse(parts[6]),
                    HasWifi = bool.Parse(parts[7]),
                    Units = AccommodationUnitRepository.GetByAccommodationId(int.Parse(parts[0])),
                    IsDeleted = isDeleted
                };

                accommodations.Add(accommodation);
            }

            return accommodations;
        }

        public static List<Accommodation> GetActiveOnly()
        {
            return GetAll().Where(a => !a.IsDeleted).ToList();
        }

        public static Dictionary<int, List<Accommodation>> GetAllGrouped()
        {
            var accommodationsByArrangement = new Dictionary<int, List<Accommodation>>();

            if (!File.Exists(filePath))
                return accommodationsByArrangement;

            var lines = File.ReadAllLines(filePath);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length < 9)
                    continue;

                bool isDeleted = false;
                if (parts.Length > 9)
                    bool.TryParse(parts[9], out isDeleted);

                if (isDeleted) 
                    continue; 

                int arrangementId = int.Parse(parts[8]);

                var acc = new Accommodation
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1],
                    Type = (AccommodationTypeEnum)Enum.Parse(typeof(AccommodationTypeEnum), parts[2], true),
                    Stars = int.Parse(parts[3]),
                    HasPool = bool.Parse(parts[4]),
                    HasSpa = bool.Parse(parts[5]),
                    Accessible = bool.Parse(parts[6]),
                    HasWifi = bool.Parse(parts[7]),
                    Units = AccommodationUnitRepository.GetByAccommodationId(int.Parse(parts[0])),
                    IsDeleted = isDeleted
                };

                if (!accommodationsByArrangement.ContainsKey(arrangementId))
                    accommodationsByArrangement[arrangementId] = new List<Accommodation>();

                accommodationsByArrangement[arrangementId].Add(acc);
            }

            return accommodationsByArrangement;
        }

        public static List<Accommodation> GetByArrangementId(int arrangementId)
        {
            var grouped = GetAllGrouped();
            if (grouped.ContainsKey(arrangementId))
                return grouped[arrangementId];
            else
                return new List<Accommodation>();
        }

        public static List<Accommodation> GetByManagerUsername(string managerUsername)
        {
            var arrangements = ArrangementRepository.GetAllByManagerUsername(managerUsername);
            var accommodations = new List<Accommodation>();

            foreach (var arrangement in arrangements)
            {
                var accoms = GetByArrangementId(arrangement.Id);
                accommodations.AddRange(accoms);
            }

            return accommodations.GroupBy(a => a.Id).Select(g => g.First()).ToList(); // Ukloni duplikate
        }

        public static Accommodation GetById(int id)
        {
            return GetAll().FirstOrDefault(a => a.Id == id);
        }

        public static Accommodation GetByName(string name)
        {
            var allAccommodations = GetAll();
            return allAccommodations.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static int GetNextId()
        {
            var accommodations = GetAll();
            return accommodations.Any() ? accommodations.Max(a => a.Id) + 1 : 1;
        }

        public static void Add(Accommodation accommodation, int arrangementId)
        {
            bool fileExists = File.Exists(filePath);

            using (var sw = new StreamWriter(filePath, true))
            {
                if (!fileExists)
                {
                    sw.WriteLine("ID;Name;Type;Stars;HasPool;HasSpa;Accessible;HasWifi;ArragementID;IsDeleted");
                }

                string line = $"{accommodation.Id};{accommodation.Name};{accommodation.Type};{accommodation.Stars};" +
                             $"{accommodation.HasPool.ToString().ToLower()};{accommodation.HasSpa.ToString().ToLower()};" +
                             $"{accommodation.Accessible.ToString().ToLower()};{accommodation.HasWifi.ToString().ToLower()};" +
                             $"{arrangementId};{accommodation.IsDeleted.ToString().ToLower()}";

                sw.WriteLine(line);
            }
        }

        public static void Update(int id, string name, AccommodationTypeEnum type, int stars,
            bool hasPool, bool hasSpa, bool accessible, bool hasWifi)
        {
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length >= 9 && int.Parse(parts[0]) == id)
                {
                    string originalArrangementId = parts[8];
                    string isDeleted = parts.Length > 9 ? parts[9] : "false";

                    lines[i] = $"{id};{name};{type};{stars};" +
                              $"{hasPool.ToString().ToLower()};{hasSpa.ToString().ToLower()};" +
                              $"{accessible.ToString().ToLower()};{hasWifi.ToString().ToLower()};" +
                              $"{originalArrangementId};{isDeleted}";
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
                if (parts.Length >= 9 && int.Parse(parts[0]) == id)
                {
                    parts[9] = "true";
                    lines[i] = string.Join(";", parts);

                    File.WriteAllLines(filePath, lines);
                    break;
                }
            }
        }
    }
}