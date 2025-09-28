using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Veb_Projekat.Models
{
    public class AccommodationUnit
    {
        public int Id {  get; set; }
        public int MaxGuests { get; set; }        
        public bool PetsAllowed { get; set; }   
        public decimal Price { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}