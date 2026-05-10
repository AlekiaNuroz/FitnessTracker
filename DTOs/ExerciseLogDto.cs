namespace FitnessTracker.DTOs;

public record ExerciseLogDto(
    int Id,
    int ExerciseId,
    string ExerciseName,
    int Week,
    string Day,
    double PrescribedWeightLbs,
    double ActualWeightLbs,
    int PrescribedSets,
    string PrescribedReps,
    bool Completed,
    int? ActualReps,
    DateTime LoggedAt);

public record LogExerciseRequest(
    int ExerciseId,
    double PrescribedWeightLbs,
    double ActualWeightLbs,
    int PrescribedSets,
    string PrescribedReps,
    bool Completed,
    int? ActualReps);