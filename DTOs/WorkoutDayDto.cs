namespace FitnessTracker.DTOs;

public record WorkoutDayDto(
    string Day,
    IEnumerable<ExerciseDto> Exercises);