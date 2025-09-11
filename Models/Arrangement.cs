using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Veb_Projekat.Models.Enums
using System.Linq;
using System.Web;

namespace Veb_Projekat.Models
{
    public class Arrangement
    {
        public string Name { get; set; } = string.Empty;

        public ArrangementTypeEnum Type { get; set; }

        public TransportTypeEnum Transport { get; set; }

        public string Location { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        public int MaxNumOfPassengers { get; set; }

        public string Description { get; set; } = string.Empty;  

        public string TravelProgram { get; set; } = string.Empty; 

        public string Poster { get; set; } = string.Empty;       

        public List<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
    }
}