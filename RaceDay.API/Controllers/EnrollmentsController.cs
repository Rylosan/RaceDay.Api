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
public class EnrollmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrollmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Participant enrols in an event
    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CreateEnrollment(
        CreateEnrollmentDto dto)
    {
        var participantId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == dto.EventId);

        if (raceEvent == null)
            return NotFound("Event not found.");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CategoryId &&
                c.EventId == dto.EventId);

        if (category == null)
            return BadRequest(
                "The selected category does not belong to this event.");

        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e =>
                e.EventId == dto.EventId &&
                e.ParticipantId == participantId);

        if (alreadyEnrolled)
            return BadRequest(
                "You are already enrolled in this event.");

        var enrollment = new Enrollment
        {
            EventId = dto.EventId,
            ParticipantId = participantId,
            CategoryId = dto.CategoryId,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            enrollment.Id,
            enrollment.EventId,
            enrollment.ParticipantId,
            enrollment.CategoryId,
            enrollment.EnrolledAt
        });
    }

    // Participant views their own enrolments
    [HttpGet("me")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var participantId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrollments = await _context.Enrollments
            .Where(e => e.ParticipantId == participantId)
            .Select(e => new
            {
                e.Id,
                e.EventId,
                EventName = e.Event.Name,
                e.CategoryId,
                CategoryName = e.Category.Name,
                e.EnrolledAt
            })
            .ToListAsync();

        return Ok(enrollments);
    }

    // Participant cancels their own enrolment
    [HttpDelete("{id}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        var participantId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
            return NotFound("Enrollment not found.");

        if (enrollment.ParticipantId != participantId)
            return Forbid();

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}