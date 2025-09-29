using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;

namespace Veb_Projekat.Services
{
    public class UserService
    {
        public static List<User> SearchUsers(List<User> users, string searchName = "", string searchLastName = "",
            string roleFilter = "")
        {
            var result = new List<User>();

            foreach (var user in users)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(searchName) &&
                    user.FirstName.IndexOf(searchName, StringComparison.OrdinalIgnoreCase) < 0)
                    match = false;

                if (!string.IsNullOrEmpty(searchLastName) &&
                    user.LastName.IndexOf(searchLastName, StringComparison.OrdinalIgnoreCase) < 0)
                    match = false;

                if (!string.IsNullOrEmpty(roleFilter))
                {
                    if (Enum.TryParse(roleFilter, out RoleEnum role))
                    {
                        if (user.UserRole != role)
                            match = false;
                    }
                }

                if (match)
                    result.Add(user);
            }

            return result;
        }

        public static bool ValidateManagerData(string username, string password, string firstName,
            string lastName, string email, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Username is required.";
                return false;
            }

            if (username.Length < 3)
            {
                errorMessage = "Username must be at least 3 characters long.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Password is required.";
                return false;
            }

            if (password.Length < 4)
            {
                errorMessage = "Password must be at least 4 characters long.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                errorMessage = "First name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                errorMessage = "Last name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Email is required.";
                return false;
            }

            return true;
        }
    }
}