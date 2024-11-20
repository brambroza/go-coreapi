using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class  Jobtype  
    {     
    public string UpdUser { get; set; } 
    public string JobTypeCode { get; set; }
    public string JobTypeName { get; set; }
    public string JobTypeDescripton { get; set; }
    public int JobTypeStateActive { get; set; }
    public string CmpId {get;set;}
    }
}