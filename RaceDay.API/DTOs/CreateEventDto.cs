using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateEventDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public decimal DistanceKm { get; set; }

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;
}