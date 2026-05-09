using FitnessTracker.Data;
using FitnessTracker.DTOs;
using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Endpoints;

public static class ProgressEndpoints
{
    public static void MapProgressEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/progress");

        // Get all progress for a user
        group.MapGet("/{username}", async (string username, AppDbContext db) =>
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
                return Results.NotFound($"User '{username}' not found.");

            var progress = await db.WorkoutProgress
                .Where(p => p.UserId == user.Id)
                .OrderBy(p => p.Week)
                .ThenBy(p => p.Day)
                .Select(p => new ProgressDto(
                    p.Id,
                    p.UserId,
                    p.Week,
                    p.Day,
                    p.Completed,
                    p.CompletedAt))
                .ToListAsync();

            return Results.Ok(progress);
        });

        // Mark a day complete
        group.MapPost("/{username}/{week}/{day}", async (
            string username, int week, string day, AppDbContext db) =>
        {
            if (week < 1 || week > 52)
                return Results.BadRequest("Week must be between 1 and 52.");

            if (!new[] { "A", "B", "C" }.Contains(day.ToUpper()))
                return Results.BadRequest("Day must be A, B, or C.");

            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
                return Results.NotFound($"User '{username}' not found.");

            var existing = await db.WorkoutProgress
                .FirstOrDefaultAsync(p =>
                    p.UserId == user.Id &&
                    p.Week == week &&
                    p.Day == day.ToUpper());

            if (existing is not null)
                return Results.Conflict($"Week {week} Day {day} is already marked complete.");

            var progress = new WorkoutProgress
            {
                UserId = user.Id,
                Week = week,
                Day = day.ToUpper(),
                Completed = true,
                CompletedAt = DateTime.UtcNow
            };

            db.WorkoutProgress.Add(progress);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/progress/{username}/{week}/{day}",
                new ProgressDto(
                    progress.Id,
                    progress.UserId,
                    progress.Week,
                    progress.Day,
                    progress.Completed,
                    progress.CompletedAt));
        });

        // Unmark a day (undo)
        group.MapDelete("/{username}/{week}/{day}", async (
            string username, int week, string day, AppDbContext db) =>
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
                return Results.NotFound($"User '{username}' not found.");

            var progress = await db.WorkoutProgress
                .FirstOrDefaultAsync(p =>
                    p.UserId == user.Id &&
                    p.Week == week &&
                    p.Day == day.ToUpper());

            if (progress is null)
                return Results.NotFound($"Week {week} Day {day} not found.");

            db.WorkoutProgress.Remove(progress);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}