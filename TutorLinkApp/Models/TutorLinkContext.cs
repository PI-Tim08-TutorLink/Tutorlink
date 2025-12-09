using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TutorLinkApp.Models;

public partial class TutorLinkContext : DbContext
{
    public TutorLinkContext()
    {
    }

    public TutorLinkContext(DbContextOptions<TutorLinkContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tutor> Tutors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=DESKTOP-EI9LFRG;User Id=sa;Password=Pa55w.rd;Database=TutorLink;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.Role1)
                .HasMaxLength(10)
                .HasColumnName("Role");
        });

        modelBuilder.Entity<Tutor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tutor__3214EC0791BC27AF");

            entity.ToTable("Tutor");

            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Skill).HasMaxLength(256);

            entity.HasOne(d => d.User).WithMany(p => p.Tutors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Tutor_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E4053021E7").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D1053435A18D38").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
            entity.Property(e => e.PwdHash).HasMaxLength(256);
            entity.Property(e => e.PwdSalt).HasMaxLength(256);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
