using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class Rolegroup
    {
        public int id { get; set; }
        public string name { get; set; }

        public List<RoleMenu> children { get;set;}
    }
}