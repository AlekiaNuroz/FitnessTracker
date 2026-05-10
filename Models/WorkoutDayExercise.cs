namespace FitnessTracker.Models;

public class WorkoutDayExercise
{
    public int Id { get; set; }
    public string Day { get; set; } = string.Empty;
    public int ExerciseId { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public Exercise Exercise { get; set; } = null!;
}