using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class STServiceActions
    {
        public string UpdUser { get; set; }
        public string ServiceActionId { get; set; }
        public string ProblemId { get; set; }
        public string? ServiceActionBy { get; set; }
        public int ServiceType { get; set; }
        public string ActionDetails { get; set; }
        public string FinishDate { get; set; }
        public string FinishTime { get; set; }
        public string CmpId { get; set; }
        public List<STServiceActions_Assign>? ActionBy { get; set; }
        public List<STServiceActions_File>? Attachfile { get; set; }

      
    }
    

   public class STServiceActions_File
    {
        public string UpdUser { get; set; }
        public string ServiceActionId { get; set; }
        public int Seq { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; } 
        public string CmpId { get; set; }

    }



    public class STServiceActions_Assign
    {
        public string UpdUser { get; set; }
        public string ServiceActionId { get; set; }
       
        public string UserId { get; set; }
        public string CmpId { get; set; }
        public string UserFullName { get; set; }
        public string ImgPath { get; set; }
        public string Permission { get; set; }
        public string RouteId { get; set; }
        public string RemindId { get; set; }


    }



    public class STServiceAction_Emp
    {
        public string Username { get; set; }
    }
}