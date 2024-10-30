using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace coreapi.Models
{
    public class Remind
    {
        public string RouteId { get; set; }
        public string RemindId {get;set;}
        public int Seq {get;set;} 
        public string RemindDescription { get; set; } 
        public int Manday { get; set; }
        public string CmpId {get;set;}



    }
}