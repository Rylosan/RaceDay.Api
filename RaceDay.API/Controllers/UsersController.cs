using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public UsersController(RaceDayDbContext context)
    {
        _context = context;
    }

    // View the currently logged-in user's profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role,
                u.Phone,
                u.DateOfBirth,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound("User not found.");

        return Ok(user);
    }

    // Update the currently logged-in user's profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        UpdateProfileDto dto)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound("User not found.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Phone = dto.Phone;
        user.DateOfBirth = dto.DateOfBirth;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.Phone,
            user.DateOfBirth,
            user.CreatedAt
        });
    }
} 