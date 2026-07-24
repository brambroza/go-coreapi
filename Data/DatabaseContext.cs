using System;
using System.Collections.Generic;
using goalongapi.Dtos;
using goalongapi.Entities;
using goalongapi.Models;
using goalongapi.Models.Nis;
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

        // REQ-004 — Self-Job Request
        public DbSet<SelfJobRequest> SelfJobRequests { get; set; }

        // ServiceTicket Master Data
        public DbSet<ServiceTicketMasterCategory> ServiceTicketMasterCategories { get; set; }
        public DbSet<ServiceTicketMasterTag> ServiceTicketMasterTags { get; set; }
        public DbSet<ServiceTicketMasterChecklist> ServiceTicketMasterChecklists { get; set; }

        // Warranty & Claims
        public DbSet<WarrantyDevice> WarrantyDevices { get; set; }
        public DbSet<WarrantyClaim> WarrantyClaims { get; set; }

        // NIS — Service Project Portal
        public DbSet<NisProject> NisProjects { get; set; }
        public DbSet<NisTicket> NisTickets { get; set; }
        public DbSet<NisProjectFile> NisProjectFiles { get; set; }
        public DbSet<NisSalesOrder> NisSalesOrders { get; set; }
        public DbSet<NisSystemConfig> NisSystemConfigs { get; set; }
        public DbSet<NisPendingRequest> NisPendingRequests { get; set; }
        public DbSet<NisOnsiteReport> NisOnsiteReports { get; set; }
        public DbSet<NisCustomerLocation> NisCustomerLocations { get; set; }
        public DbSet<NisCustomerAssignEmp> NisCustomerAssignEmps { get; set; }
        public DbSet<NisContactRow> NisContacts { get; set; }
        public DbSet<NisPersonalTodo> NisPersonalTodos { get; set; }
        public DbSet<NisPersonalNote> NisPersonalNotes { get; set; }
        public DbSet<NisPushToken> NisPushTokens { get; set; }
        public DbSet<NisPushLog> NisPushLogs { get; set; }
        public DbSet<NisOnsiteProgress> NisOnsiteProgresses { get; set; }
        public DbSet<EmailSendLog> EmailSendLogs { get; set; }

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
                entity.Property(x => x.SkipSignature).HasDefaultValue(false);
                entity.Property(x => x.RequireCloseApproval).HasDefaultValue(false);

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

                        entity.Property(e => e.WorkDetail)
                                .HasMaxLength(4000)
                                .IsRequired(false);

                        entity.Property(e => e.IssueDetail)
                                .HasMaxLength(4000)
                                .IsRequired(false);

                        entity.Property(e => e.SignatureFilePath)
                                .HasMaxLength(500)
                                .IsRequired(false);

                        entity.Property(e => e.ChecklistItemsJson)
                                .HasColumnType("nvarchar(max)")
                                .IsRequired(false);

                        entity.Property(e => e.RackPhotosJson)
                                .HasColumnType("nvarchar(max)")
                                .IsRequired(false);

                        entity.Property(e => e.DamagedProductJson)
                                .HasColumnType("nvarchar(max)")
                                .IsRequired(false);

                        entity.Property(e => e.OthersItemsJson)
                                .HasColumnType("nvarchar(max)")
                                .IsRequired(false);

                        entity.Property(e => e.SrNumber)
                                .HasMaxLength(50)
                                .IsRequired(false);

                        entity.Property(e => e.SignatureImageBase64)
                                .HasColumnType("nvarchar(max)")
                                .IsRequired(false);

                        entity.Property(e => e.WorkPhotosJson)
                                .HasColumnType("nvarchar(max)")
                                .IsRequired(false);

                        entity.Property(e => e.CheckInLatitude).HasColumnType("decimal(18,10)");
                        entity.Property(e => e.CheckInLongitude).HasColumnType("decimal(18,10)");
                        entity.Property(e => e.CheckOutLatitude).HasColumnType("decimal(18,10)");
                        entity.Property(e => e.CheckOutLongitude).HasColumnType("decimal(18,10)");

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

                   
                  entity.HasOne(e => e.TaskAction)
                    .WithMany(e => e.Attachments)
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


            // REQ-004 — Self-Job Request
            modelBuilder.Entity<SelfJobRequest>(entity =>
            {
                entity.ToTable("SelfJobRequest", "dbo");
                entity.HasKey(e => e.RequestId);

                entity.Property(e => e.RequestId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.RequestNo).HasMaxLength(50);
                entity.Property(e => e.RequestTitle).HasMaxLength(500).IsRequired();
                entity.Property(e => e.RequestType).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Draft");
                entity.Property(e => e.CmpId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CustomerCode).HasMaxLength(100);
                entity.Property(e => e.CustomerName).HasMaxLength(200);
                entity.Property(e => e.SiteName).HasMaxLength(200);
                entity.Property(e => e.ContactName).HasMaxLength(200);
                entity.Property(e => e.ContactPhone).HasMaxLength(50);
                entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("medium");
                entity.Property(e => e.ExpectedServiceDate).HasColumnType("datetime");
                entity.Property(e => e.EstimatedHours).HasColumnType("decimal(10,2)");
                entity.Property(e => e.EstimatedCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RequestedBy).HasMaxLength(100);
                entity.Property(e => e.RequestedDate).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);
                entity.Property(e => e.ApprovedDate).HasColumnType("datetime");
                entity.Property(e => e.RejectedBy).HasMaxLength(100);
                entity.Property(e => e.RejectedDate).HasColumnType("datetime");
                entity.Property(e => e.CancelledBy).HasMaxLength(100);
                entity.Property(e => e.CancelledDate).HasColumnType("datetime");
                entity.Property(e => e.TicketId).HasMaxLength(100);
                entity.Property(e => e.SubTaskId).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.CmpId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.CmpId, e.Status });
                entity.HasIndex(e => e.RequestedBy);
            });

            // ── NIS Project Portal ────────────────────────────────────────────────────

            modelBuilder.Entity<NisProject>(entity =>
            {
                entity.ToTable("NisProject", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.ProjectId);

                entity.Property(e => e.ProjectNo).HasMaxLength(50);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Customer).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CustomerCode).HasMaxLength(50);
                entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Priority).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Staff).HasMaxLength(200);
                entity.Property(e => e.SoRef).HasMaxLength(100);
                entity.Property(e => e.TagsRaw).HasMaxLength(1000);
                entity.Property(e => e.CmpId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasMany(e => e.Tickets)
                    .WithOne(e => e.Project)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Files)
                    .WithOne(e => e.Project)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CmpId);
                entity.HasIndex(e => new { e.CmpId, e.Status });
                entity.HasIndex(e => new { e.CmpId, e.ProjectNo }).IsUnique();
            });

            modelBuilder.Entity<NisProjectFile>(entity =>
            {
                entity.ToTable("NisProjectFile", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.FileId);

                entity.Property(e => e.ProjectId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FileName).HasMaxLength(300).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.CmpId).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.HasIndex(e => e.ProjectId);
            });

            modelBuilder.Entity<NisTicket>(entity =>
            {
                entity.ToTable("NisTicket", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.TicketId);

                entity.Property(e => e.ProjectId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Assignee).HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.TicketCode).HasMaxLength(50);
                entity.Property(e => e.Priority).HasMaxLength(20);
                entity.Property(e => e.TagsRaw).HasMaxLength(500);
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.Due).HasColumnType("datetime");
                entity.Property(e => e.CmpId).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.CmpId, e.Status });
                entity.HasIndex(e => e.TicketCode);
            });

            modelBuilder.Entity<NisSalesOrder>(entity =>
            {
                entity.ToTable("NisSalesOrder", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.SoId);

                entity.Property(e => e.QuoteRef).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Customer).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Date).HasColumnType("datetime");
                entity.Property(e => e.PoDate).HasColumnType("datetime");
                entity.Property(e => e.PoNumber).HasMaxLength(100);
                entity.Property(e => e.SalesName).HasMaxLength(200);
                entity.Property(e => e.CmpId).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasIndex(e => e.CmpId);
                entity.HasIndex(e => e.Status);
            });

            modelBuilder.Entity<NisPendingRequest>(entity =>
            {
                entity.ToTable("NisPendingRequest", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.RequestId);

                entity.Property(e => e.RequestedBy).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
                entity.Property(e => e.TicketType).HasMaxLength(50);
                entity.Property(e => e.SupportMethod).HasMaxLength(50);
                entity.Property(e => e.ProjectId).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.Detail).HasMaxLength(2000);
                entity.Property(e => e.ParentTicketId).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.CreatedTicketId).HasMaxLength(50);
                entity.Property(e => e.CmpId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);
                entity.Property(e => e.RejectedBy).HasMaxLength(100);
                entity.Property(e => e.Due).HasColumnType("datetime");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasIndex(e => new { e.CmpId, e.Status });
                entity.HasIndex(e => new { e.CmpId, e.RequestedBy });
            });

            modelBuilder.Entity<NisOnsiteReport>(entity =>
            {
                entity.ToTable("NisOnsiteReport", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.ReportId);

                entity.Property(e => e.NisTicketId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TicketCode).HasMaxLength(50);
                entity.Property(e => e.SrNumber).HasMaxLength(50);
                entity.Property(e => e.CmpId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Engineer).HasMaxLength(200);
                entity.Property(e => e.CheckInTime).HasMaxLength(100);
                entity.Property(e => e.CheckOutTime).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(30);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                // WorkDetail / IssueDetail / *Json / SignatureImageBase64 default to nvarchar(max).

                // Persisted Service Report PDF reference (blob stored on disk, not in-row).
                entity.Property(e => e.ReportPdfPath).HasMaxLength(400);
                entity.Property(e => e.ReportPdfSha256).HasMaxLength(64);

                entity.HasIndex(e => e.NisTicketId);
                entity.HasIndex(e => new { e.CmpId, e.SrNumber });
            });

            modelBuilder.Entity<NisPersonalTodo>(entity =>
            {
                entity.ToTable("NisPersonalTodo", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(60);
                entity.Property(e => e.CmpId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.RemindDateTime).HasMaxLength(30);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.HasIndex(e => new { e.CmpId, e.AccountId });
            });

            modelBuilder.Entity<NisPersonalNote>(entity =>
            {
                entity.ToTable("NisPersonalNote", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(60);
                entity.Property(e => e.CmpId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Reminder).HasMaxLength(30);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.HasIndex(e => new { e.CmpId, e.AccountId });
            });

            // NIS Onsite push (Track B) — table สร้างด้วย add-nis-push-tokens.sql
            modelBuilder.Entity<NisPushToken>(entity =>
            {
                entity.ToTable("NisPushToken", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CmpId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.StaffName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ExpoPushToken).HasMaxLength(255).IsRequired();
                entity.Property(e => e.DeviceId).HasMaxLength(255).IsRequired();
                // upsert key: เครื่องเดิม + คนเดิม = แถวเดิม
                entity.HasIndex(e => new { e.CmpId, e.StaffName, e.DeviceId }).IsUnique();
            });

            modelBuilder.Entity<NisPushLog>(entity =>
            {
                entity.ToTable("NisPushLog", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventKey).HasMaxLength(255).IsRequired();
                // unique = insert ซ้ำล้ม → ข้ามการส่ง (dedupe first-writer-wins)
                entity.HasIndex(e => e.EventKey).IsUnique();
            });

            modelBuilder.Entity<NisOnsiteProgress>(entity =>
            {
                entity.ToTable("NisOnsiteProgress", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CmpId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TicketId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.UserLogin).HasMaxLength(200).IsRequired();
                entity.Property(e => e.SnapshotJson).HasColumnType("nvarchar(max)").IsRequired();
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");
                // upsert key: ตั๋วเดิม + ช่างคนเดิม = แถวเดิม (last-write-wins)
                entity.HasIndex(e => new { e.CmpId, e.TicketId, e.UserLogin }).IsUnique();
                // เกณฑ์ cron ล้าง draft ค้าง (WHERE UpdatedAt < cutoff)
                entity.HasIndex(e => e.UpdatedAt);
            });

            modelBuilder.Entity<EmailSendLog>(entity =>
            {
                entity.ToTable("EmailSendLog", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CmpId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.RecipientEmail).HasMaxLength(320).IsRequired();
                entity.Property(e => e.Subject).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Provider).HasMaxLength(30).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(4000);
                entity.Property(e => e.ErrorDetail).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
                entity.HasIndex(e => new { e.CmpId, e.CreatedAt });
            });

            // Read-only mappings to existing master tables (no migration).
            modelBuilder.Entity<NisCustomerLocation>(entity =>
            {
                // Written to by the Customer tab save — disable OUTPUT for legacy triggers.
                entity.ToTable("mCustomerLocations", "msb", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => new { e.CustomerCode, e.CmpId, e.Seq });
                entity.Property(e => e.CustomerCode).HasMaxLength(50);
                entity.Property(e => e.CmpId).HasMaxLength(50);
                entity.Property(e => e.Lat).HasColumnType("numeric(18,2)");
                entity.Property(e => e.Lon).HasColumnType("numeric(18,2)");
                entity.Property(e => e.LocationName).HasMaxLength(50);
                entity.Property(e => e.Remark).HasMaxLength(500);
                entity.Property(e => e.LocationURL).HasMaxLength(200);
                entity.Property(e => e.UpdUser).HasMaxLength(50);
            });

            modelBuilder.Entity<NisCustomerAssignEmp>(entity =>
            {
                // Written to by the Customer tab caretakers matrix — disable the OUTPUT
                // clause in case the legacy table carries triggers.
                entity.ToTable("mCustomerAssignEmp", "msb", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => new { e.CustomerCode, e.CmpId, e.AccountID });
                entity.Property(e => e.CustomerCode).HasMaxLength(50);
                entity.Property(e => e.CmpId).HasMaxLength(50);
                entity.Property(e => e.UpdUser).HasMaxLength(50);
            });

            modelBuilder.Entity<NisContactRow>(entity =>
            {
                // Written to by the Customer tab save — disable OUTPUT for legacy triggers.
                entity.ToTable("Contact", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.ContactId);
                entity.Property(e => e.ContactId).HasMaxLength(50);
                entity.Property(e => e.ContactName).HasMaxLength(100);
                entity.Property(e => e.ContactEmail).HasMaxLength(100);
                entity.Property(e => e.ContactPhone).HasMaxLength(50);
                entity.Property(e => e.ContactPosition).HasMaxLength(100);
                entity.Property(e => e.ContactLineId).HasMaxLength(100);
                entity.Property(e => e.CmpId).HasMaxLength(30);
                entity.Property(e => e.DocNo).HasMaxLength(50);
                entity.Property(e => e.DocType).HasMaxLength(50);
            });

            modelBuilder.Entity<NisSystemConfig>(entity =>
            {
                entity.ToTable("NisSystemConfig", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.CmpId);

                entity.Property(e => e.CmpId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.JobTypesRaw).HasMaxLength(2000);
                entity.Property(e => e.TagsRaw).HasMaxLength(4000);
                entity.Property(e => e.ImplementChecklistRaw).HasMaxLength(8000);
                entity.Property(e => e.MaChecklistRaw).HasMaxLength(8000);
                entity.Property(e => e.PmChecklistRaw).HasMaxLength(8000);
                entity.Property(e => e.SlaOptionsRaw).HasMaxLength(500);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            // ── WarrantyClaim (table: WarrantyClaim — match add_warranty_claims_tables.sql) ──
            modelBuilder.Entity<WarrantyClaim>(entity =>
            {
                entity.ToTable("WarrantyClaim", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(30).IsRequired().ValueGeneratedNever();
                entity.Property(e => e.TicketId).HasMaxLength(100);
                entity.Property(e => e.Customer).HasMaxLength(200).IsRequired();
                entity.Property(e => e.SalesName).HasMaxLength(100);
                entity.Property(e => e.ReporterStaff).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Brand).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Model).HasMaxLength(100).IsRequired();
                entity.Property(e => e.SerialNo).HasMaxLength(100).IsRequired();
                entity.Property(e => e.WarrantyStatus).HasMaxLength(10).HasDefaultValue("on");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Claim Received");
                entity.Property(e => e.Detail).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.CmpId).HasMaxLength(50);
                entity.Property(e => e.UpdUser).HasMaxLength(100);
                entity.Property(e => e.ClaimDate).HasColumnType("date").HasDefaultValueSql("CAST(GETDATE() AS DATE)");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.CmpId);
                entity.HasIndex(e => e.ClaimDate);
            });

            // ── WarrantyDevice (table: WarrantyDevice — match add_warranty_claims_tables.sql) ──
            modelBuilder.Entity<WarrantyDevice>(entity =>
            {
                entity.ToTable("WarrantyDevice", "dbo", tb => tb.UseSqlOutputClause(false));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.SerialNo).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Brand).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Model).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Customer).HasMaxLength(200);
                entity.Property(e => e.ProjectNo).HasMaxLength(50);
                entity.Property(e => e.WarrantyStatus).HasMaxLength(10).HasDefaultValue("on");
                entity.Property(e => e.WarrantyExpiry).HasColumnType("date");
                entity.Property(e => e.CmpId).HasMaxLength(50);
                entity.Property(e => e.UpdUser).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.SerialNo).IsUnique();
                entity.HasIndex(e => e.CmpId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
