using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models.Trial
{

    public class Translation
    {
        public int Id { get; set; }
        public string Lang { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

}