namespace FitnessTracker.DTOs;

public record UserDto(
    int Id,
    string Username,
    DateTime CreatedAt,
    double CurrentWeightLbs,
    double GoalWeightLbs,
    double HeightInches);