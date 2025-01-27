using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class Users
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int StateActive { get; set; }
        public int CmpId { get; set; }
        public string ImgProfile { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CNSectId { get; set; }
        public string LineId { get; set; }  

    }
    
    public class UserMap
    {
        public int AccountID { get; set; }
        public int RoleId { get; set; }
        public string CmpId {get; set;}
    }

    public class SaleTeamMap 
    {
        public int AccountID {get;set;}
        public int SaleTeamId {get;set;}
        public string CmpId {get;set;}
    }


    public class UserActionLog
    {
        public int CmpId { get; set; }
        public string UserLogin { get; set; }
        public string ActionsDescriptions { get; set; }
        public string btnname { get; set; }
        public string MenuName { get; set; }
    }

   

}