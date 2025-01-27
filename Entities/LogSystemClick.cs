using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Numerics;
using System.Security.AccessControl;

namespace goalongapi.Entities
{
    public partial class LogSystemClick
    {
        [Key] 
        public long Seq { get; set; }
        public string UserName { get; set; }
        public string MenuName { get; set; }
        public string ObjectName { get; set; }
        public string CmpId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
