
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Participant")]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ProfileController(RaceDayDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var participantId))
            return Unauthorized();

        var participant = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == participantId);

        if (participant == null)
            return NotFound("Profile not found.");

        var profile = new ProfileDto
        {
            Id = participant.Id,
            FirstName = participant.FirstName,
            LastName = participant.LastName,
            Email = participant.Email,
            Phone = participant.Phone,
            DateOfBirth = participant.DateOfBirth,
            Role = participant.Role,
            CreatedAt = participant.CreatedAt
        };

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        UpdateProfileDto dto)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var participantId))
            return Unauthorized();

        var participant = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == participantId);

        if (participant == null)
            return NotFound("Profile not found.");

        participant.FirstName = dto.FirstName;
        participant.LastName = dto.LastName;
        participant.Phone = dto.Phone;
        participant.DateOfBirth = dto.DateOfBirth;

        await _context.SaveChangesAsync();

        var profile = new ProfileDto
        {
            Id = participant.Id,
            FirstName = participant.FirstName,
            LastName = participant.LastName,
            Email = participant.Email,
            Phone = participant.Phone,
            DateOfBirth = participant.DateOfBirth,
            Role = participant.Role,
            CreatedAt = participant.CreatedAt
        };

        return Ok(profile);
    }
}
