using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Category
{
    public int Id { get; set; }

    public int EventId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Event Event { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}