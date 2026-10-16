using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Event
{
    public int Id { get; set; }

    public int OrganiserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal DistanceKm { get; set; }

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    public User Organiser { get; set; } = null!;

    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public RouteInfo? RouteInfo { get; set; }
}