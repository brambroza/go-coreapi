using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace coreapi.Models
{
    public class RoleSet
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public int MenuId { get; set; }
        public string CmpId {get;set;}



    }


    public class SaleTeam 
    {
        public int SaleTeamId {get;set;}
        public string SaleTeamName {get;set;}
        public string CmpId {get;set;}
        public int AccountID  {get;set;}
    }

    public class setRoleGroup 
    {
        public string title {get;set;}
        public int key {get;set;}
        public string icon {get;set;}
        public List<setRoleGroup>  children {get;set;}
    }
}