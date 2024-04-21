using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class  ProblemType  
    {     
    public string UpdUser { get; set; } 
    public string ProblemTypeId { get; set; }
    public string Descriptions { get; set; }
    public string CmpId { get; set; }
    public int StateActive { get; set; }
    }
}