using System;
using System.Collections.Generic;

namespace goalongapi.Entities
{
    public partial class Role
    {
        public Role()
        {
            Accounts = new HashSet<Account>();
        }

        public int RoleId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime Created { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
