using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Models
{
    public class SessionUser
    {
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public RoleEnum UserRole { get; set; }
        public bool IsLoggedIn { get; set; } = false;

        public bool IsTourist => UserRole == RoleEnum.Tourist;
        public bool IsManager => UserRole == RoleEnum.Manager;
        public bool IsAdmin => UserRole == RoleEnum.Administrator;

        public void Login(User user)
        {
            Username = user.Username;
            FirstName = user.FirstName;
            LastName = user.LastName;
            UserRole = user.UserRole;
            IsLoggedIn = true;
        }

        public void Logout()
        {
            IsLoggedIn = false;
            Username = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
        }
    }
}