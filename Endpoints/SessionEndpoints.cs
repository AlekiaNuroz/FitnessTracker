using FitnessTracker.Data;
using FitnessTracker.DTOs;
using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sessions");

        // Get session log for a specific week and day
        group.MapGet("/{username}/{week}/{day}", async (
            string username, int week, string day, AppDbContext db) =>
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
                return Results.NotFound($"User '{username}' not found.");

            var logs = await db.ExerciseLogs
                .Where(l => l.UserId == user.Id &&
                            l.Week == week &&
                            l.Day == day.ToUpper())
                .Include(l => l.Exercise)
                .OrderBy(l => l.Exercise.SortOrder)
                .Select(l => new ExerciseLogDto(
                    l.Id,
                    l.ExerciseId,
                    l.Exercise.Name,
                    l.Week,
                    l.Day,
                    l.PrescribedWeightLbs,
                    l.ActualWeightLbs,
                    l.PrescribedSets,
                    l.PrescribedReps,
                    l.Completed,
                    l.ActualReps,
                    l.LoggedAt))
                .ToListAsync();

            return Results.Ok(logs);
        });

        // Save session log for a week and day
        group.MapPost("/{username}/{week}/{day}", async (
            string username, int week, string day,
            List<LogExerciseRequest> requests, AppDbContext db) =>
        {
            if (week < 1 || week > 52)
                return Results.BadRequest("Week must be between 1 and 52.");

            if (!new[] { "A", "B", "C" }.Contains(day.ToUpper()))
                return Results.BadRequest("Day must be A, B, or C.");

            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
                return Results.NotFound($"User '{username}' not found.");

            // Remove existing logs for this session if re-submitting
            var existing = await db.ExerciseLogs
                .Where(l => l.UserId == user.Id &&
                            l.Week == week &&
                            l.Day == day.ToUpper())
                .ToListAsync();

            if (existing.Any())
                db.ExerciseLogs.RemoveRange(existing);

            var logs = requests.Select(r => new ExerciseLog
            {
                UserId = user.Id,
                Week = week,
                Day = day.ToUpper(),
                ExerciseId = r.ExerciseId,
                PrescribedWeightLbs = r.PrescribedWeightLbs,
                ActualWeightLbs = r.ActualWeightLbs,
                PrescribedSets = r.PrescribedSets,
                PrescribedReps = r.PrescribedReps,
                Completed = r.Completed,
                ActualReps = r.ActualReps,
                LoggedAt = DateTime.UtcNow
            }).ToList();

            await db.ExerciseLogs.AddRangeAsync(logs);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/sessions/{username}/{week}/{day}",
                logs.Select(l => new ExerciseLogDto(
                    l.Id,
                    l.ExerciseId,
                    "",
                    l.Week,
                    l.Day,
                    l.PrescribedWeightLbs,
                    l.ActualWeightLbs,
                    l.PrescribedSets,
                    l.PrescribedReps,
                    l.Completed,
                    l.ActualReps,
                    l.LoggedAt)));
        });
    }
}