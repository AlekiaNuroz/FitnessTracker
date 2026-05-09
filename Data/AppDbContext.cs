using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WorkoutProgress> WorkoutProgress => Set<WorkoutProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<WorkoutProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Day).IsRequired().HasMaxLength(1);
            entity.HasIndex(e => new { e.UserId, e.Week, e.Day }).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(e => e.WorkoutProgress)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}