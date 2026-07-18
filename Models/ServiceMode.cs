using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class MServiceMode
    {

        public string? UpdUser { get; set; }

        public string CmpId { get; set; } = string.Empty;

        public string ServiceModeId { get; set; } = string.Empty;

        public string? Descriptions { get; set; }

        public int? StateActive { get; set; }

        public DateTime? UpdDate { get; set; }

        public TimeSpan? UpdTime { get; set; }

    }

}