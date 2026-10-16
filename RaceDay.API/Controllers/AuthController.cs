using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(
        RaceDayDbContext context,
        TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (dto.Role != "Organiser" && dto.Role != "Participant")
        {
            return BadRequest(
                "Role must be either Organiser or Participant.");
        }

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);

        if (emailExists)
        {
            return BadRequest("A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.CreateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var validPassword = BCrypt.Net.BCrypt.Verify(
            dto.Password,
            user.PasswordHash);

        if (!validPassword)
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = _tokenService.CreateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role
        });
    }
}