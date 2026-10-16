namespace RaceDay.API.Models;

public class Enrollment
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int ParticipantId { get; set; }

    public int CategoryId { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;

    public User Participant { get; set; } = null!;

    public Category Category { get; set; } = null!;

    public Result? Result { get; set; }
}