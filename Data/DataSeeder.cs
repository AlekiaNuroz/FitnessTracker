using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Exercises.AnyAsync()) return;

        var exercises = new List<Exercise>
        {
            // Day A — Upper Body + Core
            new() { Id = 1,  Name = "Smith Machine Incline Press",      MuscleGroup = "Chest",      BodyRegion = BodyRegion.Upper, SortOrder = 1  },
            new() { Id = 2,  Name = "Smith Machine Flat Press",          MuscleGroup = "Chest",      BodyRegion = BodyRegion.Upper, SortOrder = 2  },
            new() { Id = 3,  Name = "Pec Deck Machine",                  MuscleGroup = "Chest",      BodyRegion = BodyRegion.Upper, SortOrder = 3  },
            new() { Id = 4,  Name = "Machine Shoulder Press",            MuscleGroup = "Shoulders",  BodyRegion = BodyRegion.Upper, SortOrder = 4  },
            new() { Id = 5,  Name = "Reverse Pec Deck",                  MuscleGroup = "Shoulders",  BodyRegion = BodyRegion.Upper, SortOrder = 5  },
            new() { Id = 6,  Name = "Tricep Extension Machine",          MuscleGroup = "Triceps",    BodyRegion = BodyRegion.Upper, SortOrder = 6  },
            new() { Id = 7,  Name = "Ab Crunch Machine",                 MuscleGroup = "Core",       BodyRegion = BodyRegion.Core,  SortOrder = 7  },
            new() { Id = 8,  Name = "Torso Rotation Machine",            MuscleGroup = "Core",       BodyRegion = BodyRegion.Core,  SortOrder = 8  },

            // Day B — Lower Body
            new() { Id = 9,  Name = "Smith Machine Squat",               MuscleGroup = "Quads",      BodyRegion = BodyRegion.Lower, SortOrder = 1  },
            new() { Id = 10, Name = "Smith Machine Hip Thrust",          MuscleGroup = "Glutes",     BodyRegion = BodyRegion.Lower, SortOrder = 2  },
            new() { Id = 11, Name = "Smith Machine Romanian Deadlift",   MuscleGroup = "Hamstrings", BodyRegion = BodyRegion.Lower, SortOrder = 3  },
            new() { Id = 12, Name = "Leg Press Machine",                 MuscleGroup = "Quads",      BodyRegion = BodyRegion.Lower, SortOrder = 4  },
            new() { Id = 13, Name = "Leg Curl Machine",                  MuscleGroup = "Hamstrings", BodyRegion = BodyRegion.Lower, SortOrder = 5  },
            new() { Id = 14, Name = "Hip Abduction Machine",             MuscleGroup = "Glutes",     BodyRegion = BodyRegion.Lower, SortOrder = 6  },
            new() { Id = 15, Name = "Hip Adduction Machine",             MuscleGroup = "Inner Thigh",BodyRegion = BodyRegion.Lower, SortOrder = 7  },
            new() { Id = 16, Name = "Calf Raise Machine",                MuscleGroup = "Calves",     BodyRegion = BodyRegion.Lower, SortOrder = 8  },

            // Day C — Back, Biceps + Core
            new() { Id = 17, Name = "Lat Pulldown Machine",              MuscleGroup = "Back",       BodyRegion = BodyRegion.Upper, SortOrder = 1  },
            new() { Id = 18, Name = "Seated Row Machine",                MuscleGroup = "Back",       BodyRegion = BodyRegion.Upper, SortOrder = 2  },
            new() { Id = 19, Name = "Smith Machine Bent-Over Row",       MuscleGroup = "Back",       BodyRegion = BodyRegion.Upper, SortOrder = 3  },
            new() { Id = 20, Name = "Bicep Curl Machine",                MuscleGroup = "Biceps",     BodyRegion = BodyRegion.Upper, SortOrder = 4  },
            new() { Id = 21, Name = "Assisted Pull-Up Machine",          MuscleGroup = "Back",       BodyRegion = BodyRegion.Upper, SortOrder = 5  },
            new() { Id = 22, Name = "Torso Rotation Machine",            MuscleGroup = "Core",       BodyRegion = BodyRegion.Core,  SortOrder = 6  },
            new() { Id = 23, Name = "Ab Crunch Machine",                 MuscleGroup = "Core",       BodyRegion = BodyRegion.Core,  SortOrder = 7  },
        };

        var workoutDayExercises = new List<WorkoutDayExercise>
        {
            // Day A
            new() { Id = 1,  Day = "A", ExerciseId = 1,  SortOrder = 1 },
            new() { Id = 2,  Day = "A", ExerciseId = 2,  SortOrder = 2 },
            new() { Id = 3,  Day = "A", ExerciseId = 3,  SortOrder = 3 },
            new() { Id = 4,  Day = "A", ExerciseId = 4,  SortOrder = 4 },
            new() { Id = 5,  Day = "A", ExerciseId = 5,  SortOrder = 5 },
            new() { Id = 6,  Day = "A", ExerciseId = 6,  SortOrder = 6 },
            new() { Id = 7,  Day = "A", ExerciseId = 7,  SortOrder = 7 },
            new() { Id = 8,  Day = "A", ExerciseId = 8,  SortOrder = 8 },

            // Day B
            new() { Id = 9,  Day = "B", ExerciseId = 9,  SortOrder = 1 },
            new() { Id = 10, Day = "B", ExerciseId = 10, SortOrder = 2 },
            new() { Id = 11, Day = "B", ExerciseId = 11, SortOrder = 3 },
            new() { Id = 12, Day = "B", ExerciseId = 12, SortOrder = 4 },
            new() { Id = 13, Day = "B", ExerciseId = 13, SortOrder = 5 },
            new() { Id = 14, Day = "B", ExerciseId = 14, SortOrder = 6 },
            new() { Id = 15, Day = "B", ExerciseId = 15, SortOrder = 7 },
            new() { Id = 16, Day = "B", ExerciseId = 16, SortOrder = 8 },

            // Day C
            new() { Id = 17, Day = "C", ExerciseId = 17, SortOrder = 1 },
            new() { Id = 18, Day = "C", ExerciseId = 18, SortOrder = 2 },
            new() { Id = 19, Day = "C", ExerciseId = 19, SortOrder = 3 },
            new() { Id = 20, Day = "C", ExerciseId = 20, SortOrder = 4 },
            new() { Id = 21, Day = "C", ExerciseId = 21, SortOrder = 5 },
            new() { Id = 22, Day = "C", ExerciseId = 22, SortOrder = 6 },
            new() { Id = 23, Day = "C", ExerciseId = 23, SortOrder = 7 },
        };

        await db.Exercises.AddRangeAsync(exercises);
        await db.WorkoutDayExercises.AddRangeAsync(workoutDayExercises);
        await db.SaveChangesAsync();
    }
}