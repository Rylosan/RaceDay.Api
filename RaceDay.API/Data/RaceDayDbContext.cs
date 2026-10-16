using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Result> Results { get; set; }
    public DbSet<RouteInfo> RouteInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Each email address must be unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Organiser -> Events
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Event -> Enrollments
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Event)
            .WithMany(ev => ev.Enrollments)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Participant -> Enrollments
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Participant)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category -> Enrollments
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // A participant can only enrol once per event
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.EventId, e.ParticipantId })
            .IsUnique();

        // Enrollment -> Result (one-to-zero/one)
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrollment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Event -> RouteInfo (one-to-zero/one)
        modelBuilder.Entity<RouteInfo>()
            .HasOne(r => r.Event)
            .WithOne(e => e.RouteInfo)
            .HasForeignKey<RouteInfo>(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Distance precision for SQL Server
        modelBuilder.Entity<Event>()
            .Property(e => e.DistanceKm)
            .HasPrecision(8, 2);
    }
}