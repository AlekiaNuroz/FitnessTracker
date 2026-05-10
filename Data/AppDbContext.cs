using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WorkoutProgress> WorkoutProgress => Set<WorkoutProgress>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutDayExercise> WorkoutDayExercises => Set<WorkoutDayExercise>();
    public DbSet<ExerciseLog> ExerciseLogs => Set<ExerciseLog>();

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

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MuscleGroup).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BodyRegion)
                  .HasConversion<string>();
        });

        modelBuilder.Entity<WorkoutDayExercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Day).IsRequired().HasMaxLength(1);
            entity.HasOne(e => e.Exercise)
                  .WithMany(e => e.WorkoutDayExercises)
                  .HasForeignKey(e => e.ExerciseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExerciseLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Day).IsRequired().HasMaxLength(1);
            entity.Property(e => e.PrescribedReps).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Exercise)
                  .WithMany(e => e.ExerciseLogs)
                  .HasForeignKey(e => e.ExerciseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}