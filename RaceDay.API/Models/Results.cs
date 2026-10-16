namespace RaceDay.API.Models;

public class Result
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public TimeSpan FinishTime { get; set; }

    public int Position { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Enrollment Enrollment { get; set; } = null!;
}