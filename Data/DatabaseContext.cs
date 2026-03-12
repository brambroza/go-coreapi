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

            modelBuilder.Entity<ServiceTicket>(entity =>
        {
            entity.ToTable("ServiceTicket", tb => tb.UseSqlOutputClause(false));

            entity.HasKey(x => x.TicketId);

            entity.Property(x => x.CustomerName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.JobType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CmpId).HasMaxLength(50);
            entity.Property(x => x.UpdUser).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("draft");

            entity.HasMany(x => x.JobGroups)
                  .WithOne(x => x.ServiceTicket)
                  .HasForeignKey(x => x.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Attachments)
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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
