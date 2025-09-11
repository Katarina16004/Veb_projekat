using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Models
{
    public class Accommodation
    {
        public AccommodationTypeEnum Type { get; set; }       
        public string Name { get; set; } = string.Empty;       
        public int Stars { get; set; }    //for hotels        
        public bool HasPool { get; set; }                    
        public bool HasSpa { get; set; }                      
        public bool Accessible { get; set; }                    
        public bool HasWifi { get; set; }                      
        public List<AccommodationUnit> Units { get; set; } = new List<AccommodationUnit>();
    }
}