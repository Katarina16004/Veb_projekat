using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Repositories
{
    public class UserRepository
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Data/Users.txt");

        public static List<User> GetAll()
        {
            var users = new List<User>();

            if (!File.Exists(filePath))
                return users;

            var lines = File.ReadAllLines(filePath);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length < 8)
                    continue;

                DateTime dateOfBirth;
                DateTime.TryParseExact(parts[6], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOfBirth);

                var user = new User
                {
                    Username = parts[0],
                    Password = parts[1],
                    FirstName = parts[2],
                    LastName = parts[3],
                    Gender = Enum.TryParse(parts[4], true, out GenderEnum g) ? g : GenderEnum.Female,
                    Email = parts[5],
                    DateOfBirth = dateOfBirth,
                    UserRole = Enum.TryParse(parts[7], true, out RoleEnum r) ? r : RoleEnum.Tourist
                };

                users.Add(user);
            }

            return users;
        }

        public static User GetByUsername(string username)
        {
            return GetAll().FirstOrDefault(u => u.Username == username);
        }

        public static void Add(User user)
        {
            bool fileExists = File.Exists(filePath);

            using (var sw = new StreamWriter(filePath, true))
            {
                if (!fileExists)
                    sw.WriteLine("Username;Password;FirstName;LastName;Gender;Email;DateOfBirth;UserRole");

                string line = $"{user.Username};{user.Password};{user.FirstName};{user.LastName};{user.Gender};{user.Email};{user.DateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)};{user.UserRole}";
                sw.WriteLine(line);
            }
        }
        public static void LoadTouristReservations()
        {
            var users = GetAll();

            foreach (var user in users.Where(u => u.UserRole == RoleEnum.Tourist))
            {
                user.Reservations = ReservationRepository.GetByTouristUsername(user.Username);
            }
        }

        public static void LoadManagerArrangements()
        {
            var users = GetAll();

            foreach (var user in users.Where(u => u.UserRole == RoleEnum.Manager))
            {
                user.CreatedArrangements = ArrangementRepository.GetAllByManagerUsername(user.Username);
            }
        }

    }
}