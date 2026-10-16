using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // An organiser can create many events
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();

    // A participant can have many event enrolments
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
} 