using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Models
{
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public User Tourist { get; set; } = null;     

        public ReservationStatusEnum Status { get; set; } = ReservationStatusEnum.Active;

        public Arrangement SelectedArrangement { get; set; } = null; 
        public AccommodationUnit SelectedUnit { get; set; } = null;  
    }
}