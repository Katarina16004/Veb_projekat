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
                    Units = AccommodationUnitRepository.GetByAccommodationId(int.Parse(parts[0]))
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
        public static Accommodation GetByName(string name)
        {
            var grouped = GetAllGrouped();
            foreach (var dictionary in grouped)
            {
                foreach (var acc in dictionary.Value)
                {
                    if (acc.Name == name)
                        return acc;
                }
            }
            return null; 
        }


        public static void Add(Accommodation accommodation, int arrangementId)
        {
            bool fileExists = File.Exists(filePath);

            using (var sw = new StreamWriter(filePath, true))
            {
                if (!fileExists)
                {
                    sw.WriteLine("ID;Name;Type;Stars;HasPool;HasSpa;Accessible;HasWifi;ArragementID");
                }

                string line = $"{accommodation.Id};{accommodation.Name};{accommodation.Type};{accommodation.Stars};{accommodation.HasPool};{accommodation.HasSpa};{accommodation.Accessible};{accommodation.HasWifi};{arrangementId}";
                sw.WriteLine(line);
            }
        }
    }
}