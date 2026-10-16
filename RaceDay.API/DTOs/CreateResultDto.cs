using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateResultDto
{
    [Required]
    public int EnrollmentId { get; set; }

    [Required]
    public TimeSpan FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int Position { get; set; }
}