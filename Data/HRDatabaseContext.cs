using Microsoft.EntityFrameworkCore;
using goalongapi.Models;

namespace goalongapi.Data;

public class HrDbContext : DbContext
{

    public HrDbContext() { }
    public HrDbContext(DbContextOptions<HrDbContext> options)
        : base(options) { }

    public DbSet<ScanType> ScanTypes => Set<ScanType>();
    public DbSet<ScanTypeSlot> ScanTypeSlots => Set<ScanTypeSlot>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    public DbSet<DeviceScan> DevicesScan => Set<DeviceScan>();
    public DbSet<DeviceUserScan> DeviceUsersScan => Set<DeviceUserScan>();
    public DbSet<AttendanceDaily> AttendanceDaily => Set<AttendanceDaily>();
    public DbSet<AttendancePunch> AttendancePunches => Set<AttendancePunch>();
    public DbSet<AttendanceAdjustment> AttendanceAdjustments => Set<AttendanceAdjustment>();
    public DbSet<AttendanceRuleSet> AttendanceRuleSets => Set<AttendanceRuleSet>();
    public DbSet<OTRequest> OTRequests => Set<OTRequest>();

    public DbSet<HolidayCalendar> HolidayCalendars => Set<HolidayCalendar>();
    public DbSet<AttendanceRawLog> AttendanceRawLogs => Set<AttendanceRawLog>();


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;user id=sa; password=dr0wss@p; Database=GoAlongDatabase; TrustServerCertificate=true;"
            );
        }
    }
    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ===== ScanTypes
        mb.Entity<ScanType>(e =>
        {
            e.ToTable("ScanTypes", "hr");
            e.HasKey(x => x.ScanTypeId);

            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.PunchCount).IsRequired();
            e.Property(x => x.HasOT).IsRequired();
            e.Property(x => x.IsStrictOrder).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(1000);

            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysutcdatetime()");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysutcdatetime()");
            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();

            e.HasMany(x => x.Slots)
             .WithOne(x => x.ScanType!)
             .HasForeignKey(x => x.ScanTypeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== ScanTypeSlots
        mb.Entity<ScanTypeSlot>(e =>
        {
            e.ToTable("ScanTypeSlots", "hr");
            e.HasKey(x => x.ScanTypeSlotId);

            e.Property(x => x.SeqNo).IsRequired();
            e.Property(x => x.SlotCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.SlotName).HasMaxLength(200).IsRequired();

            e.Property(x => x.ExpectedFrom).HasColumnType("time");
            e.Property(x => x.ExpectedTo).HasColumnType("time");

            e.Property(x => x.Required).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysutcdatetime()");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysutcdatetime()");
            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();

            e.HasIndex(x => new { x.CmpId, x.ScanTypeId, x.SeqNo }).IsUnique();
        });

        // ===== Shifts
        mb.Entity<Shift>(e =>
        {
            e.ToTable("Shifts", "hr");
            e.HasKey(x => x.ShiftId);

            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.StartTime).HasColumnType("time").IsRequired();
            e.Property(x => x.EndTime).HasColumnType("time").IsRequired();

            e.Property(x => x.CrossMidnight).IsRequired();
            e.Property(x => x.GraceLateMin).IsRequired();
            e.Property(x => x.GraceEarlyLeaveMin).IsRequired();
            e.Property(x => x.MinWorkMinForPresent).IsRequired();
            e.Property(x => x.IsActive).IsRequired();

            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysutcdatetime()");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysutcdatetime()");
            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();

            e.HasOne(x => x.ScanType)
             .WithMany()
             .HasForeignKey(x => x.ScanTypeId)
             .OnDelete(DeleteBehavior.NoAction);

            // ถ้ามี RowVer ใน DB
            e.Property(x => x.RowVer).IsRowVersion().IsConcurrencyToken();
        });

        mb.Entity<LeaveType>(e =>
    {
        e.ToTable("LeaveTypes", "hr");
        e.HasKey(x => x.LeaveTypeId);

        e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();
        e.Property(x => x.Code).HasMaxLength(50).IsRequired();
        e.Property(x => x.Name).HasMaxLength(200).IsRequired();

        e.HasIndex(x => new { x.CmpId, x.Code }).IsUnique();
        e.HasIndex(x => new { x.CmpId, x.IsActive });
    });

        // LeaveRequests
        mb.Entity<LeaveRequest>(e =>
        {
            e.ToTable("LeaveRequests", "hr");
            e.HasKey(x => x.LeaveId);

            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();

            e.Property(x => x.DateFrom).HasColumnType("date").IsRequired();
            e.Property(x => x.DateTo).HasColumnType("date").IsRequired();
            e.Property(x => x.TimeFrom).HasColumnType("time");
            e.Property(x => x.TimeTo).HasColumnType("time");

            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.Reason).HasMaxLength(1000);
            e.Property(x => x.AttachmentUrl).HasMaxLength(500);

            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysutcdatetime()");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysutcdatetime()");

            e.HasOne(x => x.LeaveType)
             .WithMany()
             .HasForeignKey(x => x.LeaveTypeId)
             .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(x => new { x.CmpId, x.EmployeeId, x.DateFrom, x.DateTo });
            e.HasIndex(x => new { x.CmpId, x.Status });
        });

        // dbo.Devices_scan
        mb.Entity<DeviceScan>(e =>
        {
            e.ToTable("Devices_scan", "dbo");
            e.HasKey(x => x.DeviceId);

            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.BrandModel).HasMaxLength(200);
            e.Property(x => x.Host).HasMaxLength(255).IsRequired();
            e.Property(x => x.ProtocolType).HasMaxLength(50).IsRequired();
            e.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(1000);

            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            e.Property(x => x.LastSeenAt).HasColumnType("datetime2");
            e.Property(x => x.LastSyncAt).HasColumnType("datetime2");

            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();

            e.Property(x => x.RowVer).IsRowVersion().IsConcurrencyToken();

            e.HasMany(x => x.Users)
                .WithOne(x => x.Device)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.CmpId, x.IsActive });
            e.HasIndex(x => new { x.CmpId, x.Status });
        });

        // dbo.DeviceUsers_scan
        mb.Entity<DeviceUserScan>(e =>
        {
            e.ToTable("DeviceUsers_scan", "dbo");
            e.HasKey(x => x.DeviceUserId);

            e.Property(x => x.UserCodeOnDevice).HasMaxLength(100).IsRequired();
            e.Property(x => x.CardNo).HasMaxLength(100);
            e.Property(x => x.DisplayName).HasMaxLength(200);

            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2");

            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();

            e.Property(x => x.RowVer).IsRowVersion().IsConcurrencyToken();

            e.HasIndex(x => new { x.CmpId, x.DeviceId });
            // (แนะนำ) กัน UserCode ซ้ำใน Device เดียวกัน ถ้าธุรกิจต้องการ:
            // e.HasIndex(x => new { x.CmpId, x.DeviceId, x.UserCodeOnDevice }).IsUnique();
        });


        mb.Entity<AttendanceDaily>(e =>
         {
             e.ToTable("AttendanceDaily", "hr");
             e.HasKey(x => x.AttendanceId);

             e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();
             e.Property(x => x.WorkDate).HasColumnType("date").IsRequired();

             e.Property(x => x.InTime).HasColumnType("datetimeoffset");
             e.Property(x => x.OutTime).HasColumnType("datetimeoffset");

             e.Property(x => x.Status).HasMaxLength(30).IsRequired();
             e.Property(x => x.Note).HasMaxLength(1000);

             e.Property(x => x.CalcAt).HasColumnType("datetime2");
             e.Property(x => x.CalcBy).HasMaxLength(50);

             e.Property(x => x.CreatedAt).HasColumnType("datetime2");
             e.Property(x => x.UpdatedAt).HasColumnType("datetime2");

             e.Property(x => x.RowVer).IsRowVersion().IsConcurrencyToken();

             // ✅ เพิ่มส่วนนี้: OTMinTotal เป็น computed column
             e.Property(x => x.AttendanceId)
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();

             var ot = e.Property(x => x.OTMinTotal)
                 .HasComputedColumnSql("[OTMinBeforeShift] + [OTMinAfterShift]", stored: false)
                 .ValueGeneratedOnAddOrUpdate();


             e.HasIndex(x => x.WorkDate).HasDatabaseName("IX_AttendanceDaily_WorkDate");
             e.HasIndex(x => x.Status).HasDatabaseName("IX_AttendanceDaily_Status");
             e.HasIndex(x => new { x.CmpId, x.EmployeeId, x.WorkDate });
         });

        mb.Entity<AttendancePunch>(e =>
        {
            e.ToTable("AttendancePunches", "hr");
            e.HasKey(x => x.PunchId);

            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();
            e.Property(x => x.PunchTime).HasColumnType("datetimeoffset").IsRequired();
            e.Property(x => x.PunchType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Source).HasMaxLength(30).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");

            e.HasIndex(x => new { x.CmpId, x.AttendanceId, x.PunchTime });
        });

        mb.Entity<AttendanceAdjustment>(e =>
        {
            e.ToTable("AttendanceAdjustments", "hr");
            e.HasKey(x => x.AdjustId);

            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();
            e.Property(x => x.FieldChanged).HasMaxLength(100).IsRequired();
            e.Property(x => x.OldValue).HasMaxLength(4000);
            e.Property(x => x.NewValue).HasMaxLength(4000);
            e.Property(x => x.Reason).HasMaxLength(1000);
            e.Property(x => x.CreatedBy).HasMaxLength(50);
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");

            e.HasIndex(x => new { x.CmpId, x.AttendanceId, x.CreatedAt });
        });

        mb.Entity<AttendanceRuleSet>(e =>
        {
            e.ToTable("AttendanceRuleSets", "hr");
            e.HasKey(x => x.RuleSetId);

            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.RuleJson).IsRequired();

            e.Property(x => x.EffectiveFrom).HasColumnType("date");
            e.Property(x => x.EffectiveTo).HasColumnType("date");

            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2");

            e.HasIndex(x => new { x.CmpId, x.IsDefault });
        });

        // OT Request
        mb.Entity<OTRequest>(e =>
        {
            e.ToTable("OTRequests", "hr");
            e.HasKey(x => x.OTId);

            e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();
            e.Property(x => x.WorkDate).HasColumnType("date").IsRequired();
            e.Property(x => x.TimeFrom).HasColumnType("time").IsRequired();
            e.Property(x => x.TimeTo).HasColumnType("time").IsRequired();

            e.Property(x => x.OTType).HasMaxLength(50);
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.Reason).HasMaxLength(1000);

            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2");

            e.HasIndex(x => new { x.EmployeeId, x.WorkDate }).HasDatabaseName("IX_OTRequests_Employee_WorkDate");
            e.HasIndex(x => x.Status).HasDatabaseName("IX_OTRequests_Status");
            e.HasIndex(x => new { x.CmpId, x.EmployeeId, x.WorkDate });
        });

        mb.Entity<HolidayCalendar>(e =>
         {
             e.ToTable("HolidayCalendars", "hr");
             e.HasKey(x => x.HolidayId);

             e.Property(x => x.HolidayDate).HasColumnType("date").IsRequired();
             e.Property(x => x.Name).HasMaxLength(200).IsRequired();
             e.Property(x => x.IsCompanyHoliday).IsRequired();
             e.Property(x => x.Notes).HasMaxLength(1000);

             e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();
             e.Property(x => x.Color).HasColumnType("varchar(30)").IsRequired();

             // ✅ ตรง DDL: unique HolidayDate
             // แต่ถ้าในระบบคุณเป็น multi-company จริง แนะนำให้ unique (CmpId, HolidayDate) แทน
             e.HasIndex(x => x.HolidayDate).IsUnique().HasDatabaseName("UQ_HolidayCalendars");

             e.HasIndex(x => new { x.CmpId, x.HolidayDate });
         });

        mb.Entity<AttendanceRawLog>(e =>
         {
             e.ToTable("AttendanceRawLogs", "hr");
             e.HasKey(x => x.RawLogId);

             e.Property(x => x.CmpId).HasColumnType("varchar(30)").IsRequired();

             e.Property(x => x.UserCodeOnDevice).HasMaxLength(100);
             e.Property(x => x.CardNo).HasMaxLength(100);
             e.Property(x => x.DeviceTimezone).HasMaxLength(64);
             e.Property(x => x.DeviceLogId).HasMaxLength(100);

             e.Property(x => x.DeviceLogTimeLocal).HasColumnType("datetime2(0)").IsRequired();
             e.Property(x => x.PunchTimeUtc).HasColumnType("datetimeoffset(0)");

             e.Property(x => x.TimezoneUsed).HasMaxLength(64);
             e.Property(x => x.VerifyMode).HasMaxLength(50);
             e.Property(x => x.InOutState).HasMaxLength(20);
             e.Property(x => x.WorkCode).HasMaxLength(50);

             e.Property(x => x.Source).HasMaxLength(30).IsRequired()
                 .HasDefaultValue("ZKTeco");

             e.Property(x => x.ReceivedAt).HasColumnType("datetime2(0)")
                 .HasDefaultValueSql("sysutcdatetime()");

             e.Property(x => x.IngestStatus).HasMaxLength(30).IsRequired()
                 .HasDefaultValue("New");

             e.Property(x => x.IngestError).HasMaxLength(2000);

             e.Property(x => x.UniqueHash).HasColumnType("varbinary(32)").IsRequired();

             // JOIN ไปตารางเดิม (optional)
             e.HasOne(x => x.Device)
              .WithMany()
              .HasForeignKey(x => x.DeviceId)
              .OnDelete(DeleteBehavior.NoAction);

             e.HasOne(x => x.DeviceUser)
              .WithMany()
              .HasForeignKey(x => x.DeviceUserId)
              .OnDelete(DeleteBehavior.NoAction);

             e.HasIndex(x => new { x.CmpId, x.DeviceId, x.DeviceLogTimeLocal });
             e.HasIndex(x => new { x.CmpId, x.IngestStatus, x.ReceivedAt });
         });

    }


    public override int SaveChanges()
    {
        TouchTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TouchTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void TouchTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            if (entry.Metadata.FindProperty("UpdatedAt") != null)
                entry.Property("UpdatedAt").CurrentValue = now;

            if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedAt") != null)
                entry.Property("CreatedAt").CurrentValue = now;
        }
    }
}
