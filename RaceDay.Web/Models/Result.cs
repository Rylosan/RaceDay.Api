namespace RaceDay.Web.Models;

public class Result
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public string EventName { get; set; } = "";

    public string CategoryName { get; set; } = "";

    public TimeSpan FinishTime { get; set; }

    public int Position { get; set; }
    public DateTime RecordedAt { get; set; }
}
