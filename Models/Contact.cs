using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class ContactList
    {
        public string UpdUser { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactPosition { get; set; }
        public string ContactLineId { get; set; }
        public string Remark { get; set; }
        public string ImgPath { get; set; }
        public string CmpId { get; set; }
        public string DocType { get; set; }
        public string DocNo { get; set; }
    }

    public class ContactSocail
    {
        // @SocialId varchar(150)
        public string SocialId { get; set; } = string.Empty;

        // @CmpId varchar(30)
        public string CmpId { get; set; } = string.Empty;

        // @Name NVARCHAR(150)
        public string Name { get; set; } = string.Empty;

        // @Branch NVARCHAR(150)
        public string Branch { get; set; } = string.Empty;

        // @Phone VARCHAR(50)
        public string PhoneNo { get; set; } = string.Empty;

        // @Position NVARCHAR(100)
        public string Position { get; set; } = string.Empty;

        // @Surname nvarchar(100) = ''
        public string? Surname { get; set; }

        // @Nickname nvarchar(100) = ''
        public string? Nickname { get; set; }

        // @Email nvarchar(100)  = ''
        public string? Email { get; set; }
    }

}
