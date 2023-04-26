using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class Profile
    {
    }

    public class ProfileTask
    {
        public string time { get; set; }
        public string task { get; set; }
        public string color { get; set; }
        public bool done { get; set; }
    }
}