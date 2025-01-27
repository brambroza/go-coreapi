using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace goalongapi.Models
{
    public class RoleSet
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public int MenuId { get; set; }
        public string CmpId { get; set; }
    }

    public class Rolelist
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; } 
        public string CmpId { get; set; }
        public int StateManager { get; set; }
        public int JobDesc { get; set; }
        public string JobDescFilter {get;set;}
    }

    public class JobDesc
    {
        public int JobDescId { get; set; }
        public string CmpId { get; set; }
        public string JobDescName { get; set; }
    }

    public class PermissionMenu  {
        public string CmpId {get;set;}
        public int RoleId {get;set;}
        public int MenuId {get;set;}
        public int StateActive {get;set;}
    }

   public class PermissionMenuObject  {
        public string CmpId {get;set;}
        public int RoleId {get;set;}
        public int MenuId {get;set;}
        public int StateActive {get;set;}
        public string ObjectName  {get;set;}
    }

    public class Menulist {
        public int MenuId {get;set;}
        public int MenuMainId {get;set;}
        public string title {get;set;}
        public int StateActive {get;set;}
        public int Seq {get;set;}
        public int JobDesId {get;set;}
        public int StateSelect {get;set;}
        public List<MenuButtonObject> objects {get;set;}
        
    }

    public class MenuButtonObject {
        public int MenuId {get;set;}
        public string ObjectName {get;set;}
        public int StateSelect {get;set;}
        public int StateActive {get;set;}
        public int StateManager {get;set;}
        public string ObjectLable {get;set;} 

    }

    public class UserMapRole {
        public string CmpId {get;set;}
        public int AccountID {get;set;}
        public int RoleID {get;set;}
    }

    public class SaleTeam
    {
        public int SaleTeamId { get; set; }
        public string SaleTeamName { get; set; }
        public string CmpId { get; set; }
        public int AccountID { get; set; }
    }

    public class setRoleGroup
    {
        public string title { get; set; }
        public int key { get; set; }
        public string icon { get; set; }
        public List<setRoleGroup> children { get; set; }
    }
}
