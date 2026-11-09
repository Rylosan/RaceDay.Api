
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EventsController : Controller
{
    private readonly RaceDayApiService _api;

    public EventsController(RaceDayApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        try
        {
            var events = await _api.GetAsync<List<EventModel>>(
                "api/Events", token);

            return View(events ?? new List<EventModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "Unable to retrieve events from the API.";
            return View(new List<EventModel>());
        }
    }

    // Display an event image through the authenticated API
    [HttpGet]
    public async Task<IActionResult> Image(int id)
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return Unauthorized();

        try
        {
            var image = await _api.GetImageAsync(
                $"api/Events/{id}/image", token);

            if (image == null)
                return NotFound();

            return File(image.Value.Content, image.Value.ContentType);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Unable to retrieve the event image.");
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventModel model)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        if (!ModelState.IsValid)
            return View(model);

        var token = HttpContext.Session.GetString("Token");

        try
        {
            await _api.PostAsync<CreateEventModel, System.Text.Json.JsonElement>(
                "api/Events", model, token);

            TempData["Success"] = "Event created successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                "",
                "Could not create the event. Please check your details.");

            return View(model);
        }
    }
}
