using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;

namespace SkillHub.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Workshop configuration
        builder.Entity<Workshop>(entity =>
        {
            entity.ToTable("Workshops");
            entity.HasKey(w => w.Id);

            entity.Property(w => w.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(w => w.Code)
                .IsUnique();

            entity.Property(w => w.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(w => w.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(w => w.Capacity)
                .IsRequired();

            entity.Property(w => w.StartDateTime)
                .IsRequired();

            entity.Property(w => w.EndDateTime)
                .IsRequired();

            entity.Property(w => w.CreatedAt)
                .IsRequired();

            entity.HasOne(w => w.Instructor)
                .WithMany(u => u.ConductedWorkshops)
                .HasForeignKey(w => w.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Enrollment configuration
        builder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.EnrolledAt)
                .IsRequired();

            entity.HasOne(e => e.Workshop)
                .WithMany(w => w.Enrollments)
                .HasForeignKey(e => e.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Participant)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CancelledBy)
                .WithMany()
                .HasForeignKey(e => e.CancelledById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.WorkshopId, e.ParticipantId, e.Status });
        });
    }
}
