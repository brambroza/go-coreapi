using System;
using System.Collections.Generic;
using goalongapi.Dtos;
using goalongapi.Entities;
using goalongapi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace goalongapi.Data
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext() { }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<AccountGoogle> AccountsGoogle { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<LogSystemClick> LogSystemClick { get; set; } = null!;

        public virtual DbSet<AccountSession> AccountSessions { get; set; } = null!;

        public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();

        public DbSet<ServiceTicket> ServiceTickets => Set<ServiceTicket>();
        public DbSet<ServiceTicketJobGroup> ServiceTicketJobGroups => Set<ServiceTicketJobGroup>();
        public DbSet<ServiceTicketAttachment> ServiceTicketAttachments => Set<ServiceTicketAttachment>();

        public DbSet<TeamServiceSpResult> TeamServiceSpResults { get; set; }
        public DbSet<Customer> customers { get; set; }

        public DbSet<ServiceTicketSubTask> ServiceTicketSubTasks { get; set; }
        public DbSet<ServiceTicketSubTaskAssign> ServiceTicketSubTaskAssigns { get; set; }
        public DbSet<ServiceTicketSubTaskFile> ServiceTicketSubTaskFiles { get; set; }

        public DbSet<ServiceTicketSubTaskCheckIn> ServiceTicketSubTaskCheckIns { get; set; }

        public DbSet<ServiceTicketSubTaskAction> ServiceTicketSubTaskActions { get; set; }

        public DbSet<ServiceTicketSubTaskActionAttachment> ServiceTicketSubTaskActionAttachments { get; set; }


        public DbSet<MServiceMode> MServiceModes { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=localhost,1433;user id=sa; password=dr0wss@p; Database=GoAlongDatabase; TrustServerCertificate=true;"
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.Created).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity
                    .HasOne(d => d.Role)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Accounts_Roles");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.Created).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.Created).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Image).HasMaxLength(50);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");

                entity
                    .HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Products_Categories");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.Created).HasDefaultValueSql("(getdate())");
            });



            modelBuilder.Entity<AccountSession>(e =>
                {
                    e.ToTable("AccountSessions");
                    e.HasKey(x => x.SessionId);

                    e.Property(x => x.DeviceId).HasMaxLength(64).IsRequired();
                    e.Property(x => x.DeviceName).HasMaxLength(128);
                    e.Property(x => x.UserAgent).HasMaxLength(512);
                    e.Property(x => x.IpAddress).HasMaxLength(45);

                    e.HasOne(x => x.Account)
                        .WithMany()
                        .HasForeignKey(x => x.AccountID);
                });

            modelBuilder.Entity<ReportTemplate>()
                       .HasIndex(x => new { x.TemplateCode, x.Version })
                       .IsUnique();

            modelBuilder.Entity<ReportTemplate>()
                .HasIndex(x => new { x.TemplateCode, x.IsActive });

            modelBuilder.Entity<Customer>(entity =>
                {
                    entity.ToTable("mCustomer", "msb");

                    entity.HasKey(x => new { x.CmpId, x.CustomerCode });

                    entity.Property(x => x.CmpId).HasMaxLength(50);
                    entity.Property(x => x.CustomerCode).HasMaxLength(50);
                    entity.Property(x => x.CustomerName).HasMaxLength(255);
                    entity.Property(x => x.ImgPath).HasMaxLength(200);
                });

            modelBuilder.Entity<ServiceTicket>(entity =>
            {
                entity.ToTable("ServiceTicket", tb => tb.UseSqlOutputClause(false));

                entity.HasKey(x => x.TicketId);

                entity.Property(x => x.CustomerName).HasMaxLength(255).IsRequired();
                entity.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.JobType).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Priority).HasMaxLength(20).IsRequired();
                entity.Property(x => x.CmpId).HasMaxLength(50);
                entity.Property(x => x.UpdUser).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("draft");

                entity.Ignore(x => x.ImagePath);

                entity.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => new { x.CmpId, x.CustomerCode })
                    .HasPrincipalKey(x => new { x.CmpId, x.CustomerCode })
                    .OnDelete(DeleteBehavior.NoAction);



                entity.HasMany(x => x.JobGroups)
                    .WithOne(x => x.ServiceTicket)
                    .HasForeignKey(x => x.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Attachments)
                    .WithOne(x => x.ServiceTicket)
                    .HasForeignKey(x => x.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.SubTasks)
                    .WithOne(x => x.ServiceTicket)
                    .HasForeignKey(x => x.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<ServiceTicketJobGroup>(entity =>
            {
                entity.ToTable("ServiceTicketJobGroup", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(x => x.Id);

                entity.Property(x => x.JobGroup).HasMaxLength(20).IsRequired();

                entity.HasIndex(x => new { x.TicketId, x.JobGroup }).IsUnique();
            });

            modelBuilder.Entity<ServiceTicketAttachment>(entity =>
            {
                entity.ToTable("ServiceTicketAttachment", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(x => x.AttachmentId);

                entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
                entity.Property(x => x.FilePath).HasMaxLength(500);
                entity.Property(x => x.FileExt).HasMaxLength(20);
                entity.Property(x => x.ContentType).HasMaxLength(100);
                entity.Property(x => x.CreatedBy).HasMaxLength(100);
            });

            modelBuilder.Entity<TeamServiceSpResult>().HasNoKey();

            modelBuilder.Entity<ServiceTicketSubTask>(entity =>
                {
                    entity.ToTable("ServiceTicketSubTask");

                    entity.HasKey(e => e.SubTaskId);


                    entity.Property(e => e.TicketId)
                        .HasMaxLength(100)
                        .IsRequired();

                    entity.Property(e => e.Name)
                        .HasMaxLength(500)
                        .IsRequired();

                    entity.Property(e => e.Title)
                        .HasMaxLength(500)
                        .IsRequired();

                    entity.Property(e => e.Source)
                        .HasMaxLength(30)
                        .IsRequired();

                    entity.Property(e => e.DoneBy)
                        .HasMaxLength(100);

                    entity.Property(x => x.CmpId).HasMaxLength(50).IsRequired();

                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("GETDATE()");

                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("GETDATE()");

                    entity.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("pending");


                    // approve
                    entity.Property(e => e.StateApprove)
                        .HasMaxLength(1);

                    entity.Property(e => e.DateApprove)
                        .HasColumnType("datetime");

                    entity.Property(e => e.ApproveBy)
                        .HasMaxLength(150);

                    // send approve
                    entity.Property(e => e.StateSendApprove)
                        .HasMaxLength(1);

                    entity.Property(e => e.DateSendApprove)
                        .HasColumnType("datetime");

                    entity.Property(e => e.SendApproveBy)
                        .HasMaxLength(150);


                    entity.HasIndex(e => new { e.TicketId, e.Seq });

                    entity.HasOne(e => e.ServiceTicket)
                        .WithMany(e => e.SubTasks)
                        .HasForeignKey(e => e.TicketId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasMany(e => e.Assignments)
                      .WithOne(e => e.SubTask)
                      .HasForeignKey(e => e.SubTaskId)
                      .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity<ServiceTicketSubTaskAssign>(entity =>
                {
                    entity.ToTable("ServiceTicketSubTaskAssign", tb => tb.UseSqlOutputClause(false));

                    entity.HasKey(e => e.AssignId);

                    entity.Property(e => e.AssignId)
                        .ValueGeneratedNever();

                    entity.Property(e => e.TicketId)
                        .HasMaxLength(100)
                        .IsRequired();

                    entity.Property(e => e.AssignUserId)
                        .HasMaxLength(100)
                        .IsRequired();

                    entity.Property(e => e.AssignUserName)
                        .HasMaxLength(255);

                    entity.Property(e => e.RoleName)
                        .HasMaxLength(50);

                    entity.Property(e => e.AssignedBy)
                        .HasMaxLength(100)
                        .IsRequired();

                    entity.Property(e => e.UnassignedBy)
                        .HasMaxLength(100);

                    entity.Property(e => e.IsActive)
                        .HasDefaultValue(true);

                    entity.Property(e => e.AssignedAt)
                        .HasDefaultValueSql("GETDATE()");

                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("GETDATE()");

                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("GETDATE()");

                    entity.HasIndex(e => e.SubTaskId);
                    entity.HasIndex(e => e.TicketId);
                    entity.HasIndex(e => new { e.SubTaskId, e.AssignUserId, e.IsActive });

                    entity.HasOne(e => e.SubTask)
                        .WithMany(e => e.Assignments)
                        .HasForeignKey(e => e.SubTaskId)
                        .OnDelete(DeleteBehavior.Cascade);
                });


            modelBuilder.Entity<ServiceTicketSubTaskFile>(entity =>
                {
                    entity.ToTable("ServiceTicketSubTaskFile");

                    entity.HasKey(x => x.FileId);

                    entity.Property(x => x.CmpId).HasMaxLength(50).IsRequired();
                    entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
                    entity.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();
                    entity.Property(x => x.UpdUser).HasMaxLength(100).IsRequired();

                    entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                    entity.HasIndex(x => x.SubTaskId);

                    entity.HasOne(x => x.SubTask)
                        .WithMany(x => x.AttachFiles)
                        .HasForeignKey(x => x.SubTaskId)
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity<ServiceTicketSubTaskCheckIn>(entity =>
                        {
                            entity.ToTable("ServiceTicketSubTaskCheckIn", "dbo");

                            entity.HasKey(e => e.CheckInId)
                                  .HasName("PK_ServiceTicketTaskCheckIn");

                            entity.Property(e => e.CheckInId)
                                  .HasDefaultValueSql("(newid())");

                            entity.Property(e => e.CmpId)
                                  .HasMaxLength(30)
                                  .IsRequired();

                            entity.Property(e => e.CheckInAt)
                                  .HasColumnType("datetime2(0)");

                            entity.Property(e => e.CheckOutAt)
                                  .HasColumnType("datetime2(0)");

                            entity.Property(e => e.UpdatedAt)
                                  .HasColumnType("datetime2(0)");

                            entity.Property(e => e.Latitude)
                                  .HasColumnType("decimal(18,10)");

                            entity.Property(e => e.Longitude)
                                  .HasColumnType("decimal(18,10)");

                            entity.Property(e => e.LocationText)
                                  .HasMaxLength(500);

                            entity.Property(e => e.CheckInBy)
                                  .HasMaxLength(100);

                            entity.Property(e => e.CheckOutBy)
                                  .HasMaxLength(100);

                            entity.Property(e => e.UpdatedBy)
                                  .HasMaxLength(100);

                            entity.HasIndex(e => new { e.CmpId, e.TicketId, e.SubTaskId });
                        });

            modelBuilder.Entity<ServiceTicketSubTaskAction>(entity =>
                    {
                        entity.ToTable("ServiceTicketSubTaskAction", "dbo");

                        entity.HasKey(e => e.TaskActionId)
                                .HasName("PK_ServiceTicketTaskAction");

                        entity.Property(e => e.TaskActionId)
                                .ValueGeneratedNever();

                        entity.Property(e => e.TicketId)
                                .HasMaxLength(100)
                                .IsRequired();

                        entity.Property(e => e.SubTaskId)
                                .HasMaxLength(100)
                                .IsRequired();

                        entity.Property(e => e.CmpId)
                                .HasMaxLength(30)
                                .IsRequired();

                        entity.Property(e => e.ActionDate)
                                .HasColumnType("date");

                        entity.Property(e => e.ActionDetails)
                                .HasMaxLength(500);

                        entity.Property(e => e.ActionStatus)
                                .HasMaxLength(100);

                        entity.Property(e => e.Tomorrow)
                                .HasMaxLength(100);

                        entity.Property(e => e.UpdatedAt)
                                .HasColumnType("datetime2(0)");

                        entity.HasIndex(e => new { e.CmpId, e.TicketId, e.SubTaskId });
                        entity.HasIndex(e => new { e.SubTaskId, e.Seq });
                    });


            modelBuilder.Entity<ServiceTicketSubTaskActionAttachment>(entity =>
              {
                  entity.ToTable("ServiceTicketSubTaskActionAttachment", "dbo");

                  entity.HasKey(e => new { e.AttachmentId, e.TaskActionId })
                        .HasName("PK_ServiceTicketSubTaskActionAttachment");

                  entity.Property(e => e.AttachmentId)
                        .ValueGeneratedNever();

                  entity.Property(e => e.TaskActionId)
                        .IsRequired();

                  entity.Property(e => e.FileName)
                        .HasMaxLength(255)
                        .IsRequired();

                  entity.Property(e => e.FilePath)
                        .HasMaxLength(500);

                  entity.Property(e => e.FileExt)
                        .HasMaxLength(20);

                  entity.Property(e => e.ContentType)
                        .HasMaxLength(100);

                  entity.Property(e => e.CreatedBy)
                        .HasMaxLength(100);

                  entity.Property(e => e.CreatedAt)
                        .HasColumnType("datetime2(0)");

                  entity.HasIndex(e => new { e.TaskActionId, e.Seq });

                  // ถ้ามี table แม่อยู่แล้ว แนะนำเปิด FK นี้
                  entity.HasOne<ServiceTicketSubTaskAction>()
                        .WithMany()
                        .HasForeignKey(e => e.TaskActionId)
                        .OnDelete(DeleteBehavior.Cascade);
              });

            modelBuilder.Entity<MServiceMode>(entity =>
                {
                    entity.ToTable("mServiceMode", "dbo");

                    entity.HasKey(e => new { e.CmpId, e.ServiceModeId });

                    entity.Property(e => e.CmpId)
                        .HasMaxLength(30)
                        .IsRequired();

                    entity.Property(e => e.ServiceModeId)
                        .HasMaxLength(30)
                        .IsRequired();

                    entity.Property(e => e.Descriptions)
                        .HasMaxLength(500);

                    entity.Property(e => e.UpdUser)
                        .HasMaxLength(200);
                    entity.Property(e => e.UpdDate)
                         .HasColumnType("date");

                    entity.Property(e => e.UpdTime)
                        .HasColumnType("time(7)");

                });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
