namespace FitnessTracker.DTOs;

public record ExerciseDto(
    int Id,
    string Name,
    string MuscleGroup,
    string BodyRegion,
    int SortOrder);

public record WorkoutDayExerciseDto(
    int Id,
    string Day,
    int SortOrder,
    ExerciseDto Exercise);

public record SuggestionDto(
    int ExerciseId,
    string ExerciseName,
    double SuggestedWeightLbs,
    string Source);