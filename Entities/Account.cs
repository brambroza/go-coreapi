using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Numerics;
using System.Security.AccessControl;

namespace goalongapi.Entities
{
    public partial class Account
    {
        public long AccountId { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime Created { get; set; }
        public int RoleId { get; set; }

        public string FullName { get; set; }
        public string CmpId { get; set; }
        public int stateEmailConfirm { get; set; } = 1;

        public virtual Role Role { get; set; } = null!;
        public string imgPath { get; set; }

        public string? ResetToken {get;set;}
        public DateTime? ResetTokenExpiry {get;set;}
    }
 
    public partial class AccountGoogle
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public int RoleId { get; set; }

        public string FullName { get; set; }
        public string CmpId { get; set; }
        public virtual Role Role { get; set; } = null!;
        public string imgPath { get; set; }
    }
}
