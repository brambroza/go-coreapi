using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goalongapi.Models
{
    [Table("ServiceTicketMasterCategory")]
    public class ServiceTicketMasterCategory
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public int Seq { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        [MaxLength(50)]
        public string? CmpId { get; set; }
        [MaxLength(100)]
        public string? UpdUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    [Table("ServiceTicketMasterTag")]
    public class ServiceTicketMasterTag
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public int Seq { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        [MaxLength(50)]
        public string? CmpId { get; set; }
        [MaxLength(100)]
        public string? UpdUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    [Table("ServiceTicketMasterChecklist")]
    public class ServiceTicketMasterChecklist
    {
        [Key]
        public int Id { get; set; }
        /// <summary>implement | ma</summary>
        [Required, MaxLength(50)]
        public string ChecklistType { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string Name { get; set; } = string.Empty;
        public int Seq { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        [MaxLength(50)]
        public string? CmpId { get; set; }
        [MaxLength(100)]
        public string? UpdUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
