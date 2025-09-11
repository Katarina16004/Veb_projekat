using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.DataServices.UserDataService
{
    public class UserDataService
    {
        private readonly string dataFolder = HttpContext.Current.Server.MapPath("~/App_Data/Data/");

        public List<User> LoadUsers()
        {
            var path = Path.Combine(dataFolder, "Users.txt");
            var users = new List<User>();

            if (!File.Exists(path))
                return users;

            var lines = File.ReadAllLines(path);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(';');
                if (parts.Length < 8) 
                    continue;

                users.Add(new User
                {
                    Username = parts[0],
                    Password = parts[1],
                    FirstName = parts[2],
                    LastName = parts[3],
                    Gender = (GenderEnum)Enum.Parse(typeof(GenderEnum), parts[4]),
                    Email = parts[5],
                    DateOfBirth = DateTime.ParseExact(parts[6], "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    UserRole = (RoleEnum)Enum.Parse(typeof(RoleEnum), parts[7])
                });
            }

            return users;
        }

        public List<Arrangement> LoadArrangements(List<User> users)
        {
            var path = Path.Combine(dataFolder, "Arrangements.txt");
            var arrangements = new List<Arrangement>();

            if (!File.Exists(path))
                return arrangements;

            var lines = File.ReadAllLines(path);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(';');
                if (parts.Length < 12) 
                    continue;

                var manager = users.FirstOrDefault(u => u.Username == parts[11]);

                arrangements.Add(new Arrangement
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1],
                    Type = (ArrangementTypeEnum)Enum.Parse(typeof(ArrangementTypeEnum), parts[2]),
                    Transport = (TransportTypeEnum)Enum.Parse(typeof(TransportTypeEnum), parts[3]),
                    Location = parts[4],
                    StartDate = DateTime.ParseExact(parts[5], "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    EndDate = DateTime.ParseExact(parts[6], "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    MaxNumOfPassengers = int.Parse(parts[7]),
                    Description = parts[8],
                    TravelProgram = parts[9],
                    Poster = parts[10],
                    Accommodations = new List<Accommodation>()
                });

                if (manager != null)
                    manager.CreatedArrangements.Add(arrangements.Last());
            }

            return arrangements;
        }

        public List<Reservation> LoadReservations(List<User> users, List<Arrangement> arrangements)
        {
            var path = Path.Combine(dataFolder, "Reservations.txt");
            var reservations = new List<Reservation>();

            if (!File.Exists(path))
                return reservations;

            var lines = File.ReadAllLines(path);

            foreach (var line in lines.Skip(1)) 
            {
                var parts = line.Split(';');
                if (parts.Length < 5)
                    continue; 

                var tourist = users.FirstOrDefault(u => u.Username == parts[1]);
                var arrangement = arrangements.FirstOrDefault(a => a.Name == parts[3]);

                if (tourist == null || arrangement == null)
                    continue; 

                int unitId = int.Parse(parts[4]);
                AccommodationUnit selectedUnit = null;  //to do: ucitati unite u accommodations

                if (arrangement.Accommodations != null && arrangement.Accommodations.Count > 0)
                {
                    var firstAccommodation = arrangement.Accommodations[0];
                    selectedUnit = firstAccommodation.Units.FirstOrDefault(u => u.Id == unitId);
                }

                var reservation = new Reservation
                {
                    Id = Guid.Parse(parts[0]),
                    Tourist = tourist,
                    Status = (ReservationStatusEnum)Enum.Parse(typeof(ReservationStatusEnum), parts[2]),
                    SelectedArrangement = arrangement,
                    SelectedUnit = selectedUnit
                };

                reservations.Add(reservation);

                tourist.Reservations.Add(reservation);
            }

            return reservations;
        }
    }
}