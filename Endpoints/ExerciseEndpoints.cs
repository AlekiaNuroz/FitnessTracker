using FitnessTracker.Data;
using FitnessTracker.DTOs;
using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Endpoints;

public static class ExerciseEndpoints
{
    // Formula-based weight progression by week
    private static double FormulaWeight(double baseWeight, int week, double step = 5)
    {
        double scale = week <= 13 ? 1.0 + (week - 1) * 0.02
                     : week <= 26 ? 1.26 + (week - 14) * 0.025
                     : week <= 39 ? 1.575 + (week - 27) * 0.03
                     : 1.965 + (week - 40) * 0.02;

        double raw = baseWeight * scale;
        return Math.Max(baseWeight, Math.Round(raw / step) * step);
    }

    // Base weights per exercise ID — matches the JavaScript starting weights
    private static readonly Dictionary<int, double> BaseWeights = new()
    {
        { 1,  30 },  // Smith Machine Incline Press
        { 2,  40 },  // Smith Machine Flat Press
        { 3,  40 },  // Pec Deck Machine
        { 4,  30 },  // Machine Shoulder Press
        { 5,  30 },  // Reverse Pec Deck
        { 6,  30 },  // Tricep Extension Machine
        { 7,  40 },  // Ab Crunch Machine (Day A)
        { 8,  30 },  // Torso Rotation Machine (Day A)
        { 9,  45 },  // Smith Machine Squat
        { 10, 45 },  // Smith Machine Hip Thrust
        { 11, 40 },  // Smith Machine Romanian Deadlift
        { 12, 90 },  // Leg Press Machine
        { 13, 40 },  // Leg Curl Machine
        { 14, 50 },  // Hip Abduction Machine
        { 15, 50 },  // Hip Adduction Machine
        { 16, 60 },  // Calf Raise Machine
        { 17, 50 },  // Lat Pulldown Machine
        { 18, 50 },  // Seated Row Machine
        { 19, 35 },  // Smith Machine Bent-Over Row
        { 20, 30 },  // Bicep Curl Machine
        { 21, 100 }, // Assisted Pull-Up Machine
        { 22, 30 },  // Torso Rotation Machine (Day C)
        { 23, 40 },  // Ab Crunch Machine (Day C)
    };

    public static void MapExerciseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/exercises");

        // Get all exercises for a specific day
        group.MapGet("/day/{day}", async (string day, AppDbContext db) =>
        {
            var exercises = await db.WorkoutDayExercises
                .Where(w => w.Day == day.ToUpper())
                .OrderBy(w => w.SortOrder)
                .Include(w => w.Exercise)
                .Select(w => new WorkoutDayExerciseDto(
                    w.Id,
                    w.Day,
                    w.SortOrder,
                    new ExerciseDto(
                        w.Exercise.Id,
                        w.Exercise.Name,
                        w.Exercise.MuscleGroup,
                        w.Exercise.BodyRegion.ToString(),
                        w.Exercise.SortOrder)))
                .ToListAsync();

            return Results.Ok(exercises);
        });

        // Get suggested weight for an exercise for a specific user and week
        group.MapGet("/{exerciseId}/suggestion/{username}/{week}", async (
            int exerciseId, string username, int week, AppDbContext db) =>
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
                return Results.NotFound($"User '{username}' not found.");

            var exercise = await db.Exercises.FindAsync(exerciseId);
            if (exercise is null)
                return Results.NotFound($"Exercise {exerciseId} not found.");

            // Find most recent log for this user + exercise
            var lastLog = await db.ExerciseLogs
                .Where(l => l.UserId == user.Id && l.ExerciseId == exerciseId)
                .OrderByDescending(l => l.LoggedAt)
                .FirstOrDefaultAsync();

            double suggested;
            string source;

            if (lastLog is null)
            {
                // No history — use formula
                var baseWeight = BaseWeights.GetValueOrDefault(exerciseId, 30);
                suggested = FormulaWeight(baseWeight, week);
                source = "formula";
            }
            else if (lastLog.Completed)
            {
                // Thumbs up — increase based on body region
                double increment = exercise.BodyRegion == BodyRegion.Lower ? 10 : 5;
                suggested = Math.Round((lastLog.ActualWeightLbs + increment) / 5) * 5;
                source = "progression";
            }
            else
            {
                // Thumbs down
                double prescribedRepsNum = ParseReps(lastLog.PrescribedReps);
                double actualReps = lastLog.ActualReps ?? 0;

                if (prescribedRepsNum > 0 && actualReps < prescribedRepsNum * 0.6)
                {
                    // Significantly short — decrease by 5
                    suggested = Math.Max(
                        Math.Round((lastLog.ActualWeightLbs - 5) / 5) * 5,
                        BaseWeights.GetValueOrDefault(exerciseId, 30));
                    source = "progression";
                }
                else
                {
                    // Hold current weight
                    suggested = lastLog.ActualWeightLbs;
                    source = "progression";
                }
            }

            return Results.Ok(new SuggestionDto(exerciseId, exercise.Name, suggested, source));
        });
    }

    // Parse "15", "12", "8–10" etc. to a number for comparison
    private static double ParseReps(string reps)
    {
        if (string.IsNullOrWhiteSpace(reps)) return 0;
        var parts = reps.Split('\u2013', '-');
        if (parts.Length == 2 &&
            double.TryParse(parts[0].Trim(), out var low) &&
            double.TryParse(parts[1].Trim(), out var high))
            return (low + high) / 2;
        if (double.TryParse(reps.Trim(), out var single))
            return single;
        return 0;
    }
}