using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class WareHouse {
        public string UpdUser { get; set; } 
        public int WareHouseId { get; set; } 
        public string WareHouseName { get; set; } 
        public string WareHouseDescription { get; set; } 
        public int StateActive { get; set; } 
    }
    public class WareHouseLocation { 
        public string UpdUser { get; set; } 
        public int WareHouseLocId { get; set; } 
        public int WareHouseId { get; set; } 
        public string WareHouseLocName { get; set; } 
        public string WareHouseLocDescription { get; set; } 
        public int StateActive { get; set; } 
    }

}