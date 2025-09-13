using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;

namespace Veb_Projekat.Repositories
{
    public class ReservationRepository
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Data/Reservations.txt");

        public static List<Reservation> GetAll()
        {
            var reservations = new List<Reservation>();

            if (!File.Exists(filePath))
                return reservations;

            var lines = File.ReadAllLines(filePath);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length < 5)
                    continue;

                Guid id = Guid.TryParse(parts[0], out Guid guid) ? guid : Guid.NewGuid();
                string touristUsername = parts[1];
                string statusStr = parts[2];
                string arrangementName = parts[3];
                int unitId = int.TryParse(parts[4], out int u) ? u : 0;

                var tourist = UserRepository.GetByUsername(touristUsername);
                var arrangement = ArrangementRepository.GetAll().Find(a => a.Name.Equals(arrangementName, StringComparison.OrdinalIgnoreCase));
                AccommodationUnit unit = null;

                if (arrangement != null)
                {
                    foreach (var acc in arrangement.Accommodations)
                    {
                        unit = acc.Units.Find(accu => accu.Id == unitId);
                        if (unit != null) 
                            break;
                    }
                }

                if (tourist != null && arrangement != null && unit != null)
                {
                    reservations.Add(new Reservation
                    {
                        Id = id,
                        Tourist = tourist,
                        Status = Enum.TryParse(statusStr, out Models.Enums.ReservationStatusEnum s) ? s : Models.Enums.ReservationStatusEnum.Active,
                        SelectedArrangement = arrangement,
                        SelectedUnit = unit
                    });
                }
            }

            return reservations;
        }

        public static List<Reservation> GetByTouristUsername(string username)
        {
            return GetAll().Where(r => r.Tourist.Username == username).ToList();
        }

        public static void Add(Reservation reservation)
        {
            bool fileExists = File.Exists(filePath);

            using (var sw = new StreamWriter(filePath, true))
            {
                if (!fileExists)
                    sw.WriteLine("Id;TouristUsername;Status;ArrangementName;AccommodationUnitId");

                string line = $"{reservation.Id};{reservation.Tourist.Username};{reservation.Status};{reservation.SelectedArrangement.Name};{reservation.SelectedUnit.Id}";
                sw.WriteLine(line);
            }
        }
    }
}