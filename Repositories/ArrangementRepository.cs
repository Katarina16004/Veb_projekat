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
                if (parts.Length < 13)
                    continue;

                DateTime startDate, endDate;
                DateTime.TryParseExact(parts[5], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate);
                DateTime.TryParseExact(parts[6], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate);

                bool isDeleted = false;
                bool.TryParse(parts[12], out isDeleted);

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
                    Poster = parts[10],
                    ManagerUsername = parts[11],
                    IsDeleted = isDeleted
                };

                arrangement.Accommodations = AccommodationRepository.GetByArrangementId(arrangement.Id);
                arrangements.Add(arrangement);
            }
            return arrangements;
        }

        public static List<Arrangement> GetActiveOnly()
        {
            return GetAll().Where(a => !a.IsDeleted).ToList();
        }

        public static Arrangement GetById(int id)
        {
            return GetAll().FirstOrDefault(a => a.Id == id);
        }

        public static List<Arrangement> GetActiveByManagerUsername(string username)
        {
            return GetAll().Where(a => a.ManagerUsername == username && !a.IsDeleted).ToList();
        }

        public static List<Arrangement> GetAllByManagerUsername(string username)
        {
            return GetAll().Where(a => a.ManagerUsername == username).ToList();
        }

        public static int GetNextId()
        {
            var arrangements = GetAll();
            return arrangements.Any() ? arrangements.Max(a => a.Id) + 1 : 1;
        }

        public static void Add(Arrangement arrangement)
        {
            bool fileExists = File.Exists(filePath);

            using (var sw = new StreamWriter(filePath, true))
            {
                if (!fileExists)
                {
                    sw.WriteLine("ID;Name;Type;Transport;Location;StartDate;EndDate;MaxPassengers;Description;TravelProgram;Poster;ManagerUsername;IsDeleted");
                }

                string line = $"{arrangement.Id};{arrangement.Name};{arrangement.Type};{arrangement.Transport};{arrangement.Location};" +
             $"{arrangement.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)};{arrangement.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)};{arrangement.MaxNumOfPassengers};" +
             $"{arrangement.Description};{arrangement.TravelProgram};{arrangement.Poster};{arrangement.ManagerUsername};{arrangement.IsDeleted.ToString().ToLower()}";

                sw.WriteLine(line);
            }
        }

        public static void Update(int id, string name, ArrangementTypeEnum type, TransportTypeEnum transport, string location, DateTime startDate, DateTime endDate, 
            int maxPassengers, string description, string program)
        {
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length >= 13 && int.Parse(parts[0]) == id)
                {
                    string originalPoster = parts[10];
                    string originalManager = parts[11];
                    string isDeleted = parts[12];

                    lines[i] = $"{id};{name};{type};{transport};{location};" +
                              $"{startDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)};{endDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)};{maxPassengers};" +
                              $"{description};{program};{originalPoster};{originalManager};{isDeleted}";
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
                if (parts.Length >= 13 && int.Parse(parts[0]) == id)
                {
                    parts[12] = "true";
                    lines[i] = string.Join(";", parts);

                    File.WriteAllLines(filePath, lines);
                    break;
                }
            }
        }
    }
}