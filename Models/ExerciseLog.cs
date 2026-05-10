namespace FitnessTracker.Models;

public class ExerciseLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Week { get; set; }
    public string Day { get; set; } = string.Empty;
    public int ExerciseId { get; set; }
    public double PrescribedWeightLbs { get; set; }
    public double ActualWeightLbs { get; set; }
    public int PrescribedSets { get; set; }
    public string PrescribedReps { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public int? ActualReps { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}