using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Veb_Projekat.Models
{
    public class Comment
    {
        public User Tourist { get; set; } = null;        

        public Accommodation Accommodation { get; set; } = null;

        public string Text { get; set; } = string.Empty;   

        public int Rating { get; set; }
    }
}