
namespace RaceDay.Web.Models;

public class EventResultManagementModel
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int ParticipantId { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public DateTime EnrolledAt { get; set; }

    public bool HasResult { get; set; }

    public int? ResultId { get; set; }

    public TimeSpan? FinishTime { get; set; }

    public int? Position { get; set; }
}

public class CreateResultModel
{
    public int EnrollmentId { get; set; }

    public string FinishTime { get; set; } = string.Empty;

    public int Position { get; set; }
}

public class UpdateResultModel
{
    public int ResultId { get; set; }

    public int EventId { get; set; }

    public string FinishTime { get; set; } = string.Empty;

    public int Position { get; set; }
}
