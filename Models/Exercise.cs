namespace FitnessTracker.Models;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public BodyRegion BodyRegion { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public ICollection<WorkoutDayExercise> WorkoutDayExercises { get; set; } = [];
    public ICollection<ExerciseLog> ExerciseLogs { get; set; } = [];
}

public enum BodyRegion
{
    Upper,
    Lower,
    Core
}