using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events/{eventId}/route")]
[Authorize]
public class RouteController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public RouteController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Organisers and Participants can view route information
    [HttpGet]
    public async Task<IActionResult> GetRoute(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == eventId);

        if (!eventExists)
            return NotFound("Event not found.");

        var route = await _context.RouteInfos
            .Where(r => r.EventId == eventId)
            .Select(r => new
            {
                r.Id,
                r.EventId,
                r.StartPoint,
                r.FinishPoint,
                r.RouteDescription,
                r.ElevationGainMetres
            })
            .FirstOrDefaultAsync();

        if (route == null)
            return NotFound("Route information not found.");

        return Ok(route);
    }

    // Organiser creates or updates route information
    [HttpPut]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateRoute(
        int eventId,
        RouteInfoDto dto)
    {
        var organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (raceEvent == null)
            return NotFound("Event not found.");

        if (raceEvent.OrganiserId != organiserId)
            return Forbid();

        var route = await _context.RouteInfos
            .FirstOrDefaultAsync(r => r.EventId == eventId);

        if (route == null)
        {
            route = new RouteInfo
            {
                EventId = eventId,
                StartPoint = dto.StartPoint,
                FinishPoint = dto.FinishPoint,
                RouteDescription = dto.RouteDescription,
                ElevationGainMetres = dto.ElevationGainMetres
            };

            _context.RouteInfos.Add(route);
        }
        else
        {
            route.StartPoint = dto.StartPoint;
            route.FinishPoint = dto.FinishPoint;
            route.RouteDescription = dto.RouteDescription;
            route.ElevationGainMetres = dto.ElevationGainMetres;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            route.Id,
            route.EventId,
            route.StartPoint,
            route.FinishPoint,
            route.RouteDescription,
            route.ElevationGainMetres
        });
    }
}