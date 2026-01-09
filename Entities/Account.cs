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
        public string imgPath { get; set; } = "";

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        public string? refreshToken { get; set; }
        public DateTime? refreshTokenExpiry { get; set; }
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

    public partial class AccountSession
    {
        public Guid SessionId { get; set; }
        public long AccountID { get; set; }

        public string DeviceId { get; set; } = default!;
        public string? DeviceName { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastSeenAt { get; set; }

        public DateTime ExpiresAt { get; set; }
        public byte[]? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public bool IsActive { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }
        public Guid? ReplacedBySessionId { get; set; }

        public Account? Account { get; set; }
    }

    public partial class IssueTokenResult
    {
        public string Status { get; set; } = "OK"; // OK | ALREADY_LOGGED_IN
        public string Token { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public Guid SessionId { get; set; }
        public object? ActiveSession { get; set; }
    }

}
