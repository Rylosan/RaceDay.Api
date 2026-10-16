using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoriesController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Organisers and Participants can view categories
    [HttpGet("events/{eventId}/categories")]
    [Authorize]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == eventId);

        if (!eventExists)
            return NotFound("Event not found.");

        var categories = await _context.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new
            {
                c.Id,
                c.EventId,
                c.Name,
                c.Description
            })
            .ToListAsync();

        return Ok(categories);
    }

    // Only the organiser who owns the event can add categories
    [HttpPost("events/{eventId}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateCategory(
        int eventId,
        CreateCategoryDto dto)
    {
        var organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (raceEvent == null)
            return NotFound("Event not found.");

        if (raceEvent.OrganiserId != organiserId)
            return Forbid();

        var category = new Category
        {
            EventId = eventId,
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            category.Id,
            category.EventId,
            category.Name,
            category.Description
        });
    }

    // Only the organiser who owns the event can update categories
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        CreateCategoryDto dto)
    {
        var organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound("Category not found.");

        if (category.Event.OrganiserId != organiserId)
            return Forbid();

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            category.Id,
            category.EventId,
            category.Name,
            category.Description
        });
    }

    // Only the organiser who owns the event can delete categories
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var organiserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound("Category not found.");

        if (category.Event.OrganiserId != organiserId)
            return Forbid();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}