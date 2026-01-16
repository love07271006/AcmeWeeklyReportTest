using System;
using System.Collections.Generic;
using AcmeWeeklyReportTest.Models;
using Microsoft.EntityFrameworkCore;

namespace AcmeWeeklyReportTest.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WorkLog> WorkLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);Database=AcmeTest;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Sn);

            entity.Property(e => e.Sn).HasColumnName("SN");
            entity.Property(e => e.Account).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())", "DF_Users_CreatedAt")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Users_IsActive");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<WorkLog>(entity =>
        {
            entity.HasKey(e => e.Sn);

            entity.ToTable("WorkLog");

            entity.Property(e => e.Sn).HasColumnName("SN");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())", "DF_WorkLog_CreatedAt")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserSn).HasColumnName("UserSN");
            entity.Property(e => e.WorkHours).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.UserSnNavigation).WithMany(p => p.WorkLogs)
                .HasForeignKey(d => d.UserSn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkLog_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
