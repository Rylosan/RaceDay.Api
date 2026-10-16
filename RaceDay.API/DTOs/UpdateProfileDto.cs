using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class UpdateProfileDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }
}