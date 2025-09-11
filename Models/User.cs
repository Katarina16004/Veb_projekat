using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public GenderEnum Gender { get; set; }
        public string Email { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; }
        public RoleEnum UserRole { get; set; }

        // for tourist 
        public List<Reservation> Reservations { get; set; } = new();

        // for manager
        public List<Arrangement> CreatedArrangements { get; set; } = new();
    }
}