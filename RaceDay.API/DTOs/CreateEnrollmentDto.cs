using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateEnrollmentDto
{
    [Required]
    public int EventId { get; set; }

    [Required]
    public int CategoryId { get; set; }
}