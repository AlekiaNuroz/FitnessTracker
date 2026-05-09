namespace FitnessTracker.DTOs;

public record ProgressDto(
    int Id,
    int UserId,
    int Week,
    string Day,
    bool Completed,
    DateTime CompletedAt);