namespace FitnessTracker.Models;

public class WorkoutProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Week { get; set; }
    public string Day { get; set; } = string.Empty; // "A", "B", or "C"
    public bool Completed { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}