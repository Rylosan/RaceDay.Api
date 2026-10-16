using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class RouteInfoDto
{
    [Required]
    [MaxLength(200)]
    public string StartPoint { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FinishPoint { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? RouteDescription { get; set; }

    public int? ElevationGainMetres { get; set; }
}