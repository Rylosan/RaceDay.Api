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
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Both Organisers and Participants can browse events
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .Include(e => e.Organiser)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.DistanceKm,
                e.EventType,
                Organiser = e.Organiser.FirstName + " " + e.Organiser.LastName
            })
            .ToListAsync();

        return Ok(events);
    }

    // View one event
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetEvent(int id)
    {
        var raceEvent = await _context.Events
            .Include(e => e.Organiser)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.DistanceKm,
                e.EventType,
                Organiser = e.Organiser.FirstName + " " + e.Organiser.LastName
            })
            .FirstOrDefaultAsync();

        if (raceEvent == null)
            return NotFound("Event not found.");

        return Ok(raceEvent);
    }

    // Only an Organiser can create an event
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(CreateEventDto dto)
    {
        var organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var raceEvent = new Event
        {
            OrganiserId = organiserId,
            Name = dto.Name,
            Description = dto.Description,
            EventDate = dto.EventDate,
            Location = dto.Location,
            DistanceKm = dto.DistanceKm,
            EventType = dto.EventType
        };

        _context.Events.Add(raceEvent);
        await _context.SaveChangesAsync();

        return Ok(raceEvent);
    }

    // Organiser can update only their own event
    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        UpdateEventDto dto)
    {
        var organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent == null)
            return NotFound("Event not found.");

        if (raceEvent.OrganiserId != organiserId)
            return Forbid();

        raceEvent.Name = dto.Name;
        raceEvent.Description = dto.Description;
        raceEvent.EventDate = dto.EventDate;
        raceEvent.Location = dto.Location;
        raceEvent.DistanceKm = dto.DistanceKm;
        raceEvent.EventType = dto.EventType;

        await _context.SaveChangesAsync();

        return Ok(raceEvent);
    }

    // Organiser can delete only their own event
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent == null)
            return NotFound("Event not found.");

        if (raceEvent.OrganiserId != organiserId)
            return Forbid();

        _context.Events.Remove(raceEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}