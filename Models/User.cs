namespace FitnessTracker.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Stats
    public double CurrentWeightLbs { get; set; }
    public double GoalWeightLbs { get; set; }
    public double HeightInches { get; set; }

    // Navigation
    public ICollection<WorkoutProgress> WorkoutProgress { get; set; } = [];
}