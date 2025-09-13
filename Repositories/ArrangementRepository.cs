using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Repositories
{
    public class ArrangementRepository
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Data/Arrangements.txt");

        public static List<Arrangement> GetAll()
        {
            var arrangements = new List<Arrangement>();

            if (!File.Exists(filePath))
                return arrangements;

            var lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length < 12)
                    continue;

                DateTime startDate, endDate;
                DateTime.TryParseExact(parts[5], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate);
                DateTime.TryParseExact(parts[6], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate);

                var arrangement = new Arrangement
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1],
                    Type = (ArrangementTypeEnum)Enum.Parse(typeof(ArrangementTypeEnum), parts[2], true),
                    Transport = (TransportTypeEnum)Enum.Parse(typeof(TransportTypeEnum), parts[3], true),
                    Location = parts[4],
                    StartDate = startDate,
                    EndDate = endDate,
                    MaxNumOfPassengers = int.Parse(parts[7]),
                    Description = parts[8],
                    TravelProgram = parts[9],
                    Poster = parts[10]
                };

                arrangement.Accommodations = AccommodationRepository.GetByArrangementId(arrangement.Id);

                arrangement.ManagerUsername = parts[11];

                arrangements.Add(arrangement);
            }

            return arrangements;
        }

        public static Arrangement GetById(int id)
        {
            return GetAll().FirstOrDefault(a => a.Id == id);
        }
        public static List<Arrangement> GetByManagerUsername(string username)
        {
            return GetAll().Where(a => a.ManagerUsername == username).ToList();
        }
    }
}
