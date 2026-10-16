using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Organiser records a result
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateResult(CreateResultDto dto)
    {
        var organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrollment = await _context.Enrollments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

        if (enrollment == null)
            return NotFound("Enrollment not found.");

        if (enrollment.Event.OrganiserId != organiserId)
            return Forbid();

        var resultExists = await _context.Results
            .AnyAsync(r => r.EnrollmentId == dto.EnrollmentId);

        if (resultExists)
            return BadRequest(
                "A result already exists for this enrollment.");

        var result = new Result
        {
            EnrollmentId = dto.EnrollmentId,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            RecordedAt = DateTime.UtcNow
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            result.Id,
            result.EnrollmentId,
            result.FinishTime,
            result.Position,
            result.RecordedAt
        });
    }

    // Participant views their own results
    [HttpGet("me")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        var participantId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var results = await _context.Results
            .Where(r => r.Enrollment.ParticipantId == participantId)
            .Select(r => new
            {
                r.Id,
                r.EnrollmentId,
                EventName = r.Enrollment.Event.Name,
                CategoryName = r.Enrollment.Category.Name,
                r.FinishTime,
                r.Position,
                r.RecordedAt
            })
            .ToListAsync();

        return Ok(results);
    }

    // View results for a particular event
    [HttpGet("/api/events/{eventId}/results")]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == eventId);

        if (!eventExists)
            return NotFound("Event not found.");

        var results = await _context.Results
            .Where(r => r.Enrollment.EventId == eventId)
            .Select(r => new
            {
                r.Id,
                Participant =
                    r.Enrollment.Participant.FirstName + " " +
                    r.Enrollment.Participant.LastName,
                Category = r.Enrollment.Category.Name,
                r.FinishTime,
                r.Position
            })
            .OrderBy(r => r.Position)
            .ToListAsync();

        return Ok(results);
    }
    
    // Organiser updates an existing result
[HttpPut("{id}")]
[Authorize(Roles = "Organiser")]
public async Task<IActionResult> UpdateResult(
    int id,
    UpdateResultDto dto)
{
    var organiserId = int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var result = await _context.Results
        .Include(r => r.Enrollment)
        .ThenInclude(e => e.Event)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (result == null)
        return NotFound("Result not found.");

    if (result.Enrollment.Event.OrganiserId != organiserId)
        return Forbid();

    result.FinishTime = dto.FinishTime;
    result.Position = dto.Position;
    result.RecordedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        result.Id,
        result.EnrollmentId,
        result.FinishTime,
        result.Position,
        result.RecordedAt
    });
}
}