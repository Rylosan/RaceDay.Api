
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ResultsController : Controller
{
    private readonly RaceDayApiService _apiService;

    public ResultsController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    // Participant views their own results
    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        try
        {
            var results = await _apiService.GetAsync<List<Result>>(
                "api/Results/me", token);

            return View(results ?? new List<Result>());
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to load your results.";
            return View(new List<Result>());
        }
    }

    // Organiser views participants and results for their events
    public async Task<IActionResult> Manage(int? eventId)
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        try
        {
            var events = await _apiService.GetAsync<List<EventModel>>(
                "api/Events/mine", token);

            events ??= new List<EventModel>();

            ViewBag.Events = events;
            ViewBag.SelectedEventId = eventId;

            if (!eventId.HasValue)
                return View(new List<EventResultManagementModel>());

            // Make sure the selected event belongs to this organiser
            if (!events.Any(e => e.Id == eventId.Value))
            {
                TempData["Error"] = "The selected event was not found.";
                return View(new List<EventResultManagementModel>());
            }

            var enrollments =
                await _apiService.GetAsync<List<EventResultManagementModel>>(
                    $"api/Enrollments/event/{eventId.Value}", token);

            return View(enrollments ?? new List<EventResultManagementModel>());
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to load event participants.";
            ViewBag.Events = new List<EventModel>();
            ViewBag.SelectedEventId = eventId;

            return View(new List<EventResultManagementModel>());
        }
    }

    // Organiser records a result
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int eventId,
        CreateResultModel model)
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (!TimeSpan.TryParse(
                model.FinishTime,
                CultureInfo.InvariantCulture,
                out var finishTime) ||
            finishTime <= TimeSpan.Zero)
        {
            TempData["Error"] =
                "Please enter a valid finishing time, for example 00:45:30.";

            return RedirectToAction(nameof(Manage), new { eventId });
        }

        if (model.Position < 1)
        {
            TempData["Error"] = "Position must be at least 1.";
            return RedirectToAction(nameof(Manage), new { eventId });
        }

        try
        {
            var request = new
            {
                model.EnrollmentId,
                FinishTime = finishTime,
                model.Position
            };

            await _apiService.PostAsync<object, JsonElement>(
                "api/Results", request, token);

            TempData["Success"] = "Result recorded successfully.";
        }
        catch (HttpRequestException)
        {
            TempData["Error"] =
                "Unable to record the result. It may already exist, or the request may not be valid.";
        }

        return RedirectToAction(nameof(Manage), new { eventId });
    }

    // Organiser updates an existing result
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        UpdateResultModel model)
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (!TimeSpan.TryParse(
                model.FinishTime,
                CultureInfo.InvariantCulture,
                out var finishTime) ||
            finishTime <= TimeSpan.Zero)
        {
            TempData["Error"] =
                "Please enter a valid finishing time, for example 00:45:30.";

            return RedirectToAction(
                nameof(Manage),
                new { eventId = model.EventId });
        }

        if (model.Position < 1)
        {
            TempData["Error"] = "Position must be at least 1.";

            return RedirectToAction(
                nameof(Manage),
                new { eventId = model.EventId });
        }

        try
        {
            var request = new
            {
                FinishTime = finishTime,
                model.Position
            };

            await _apiService.PutAsync<object, JsonElement>(
                $"api/Results/{model.ResultId}", request, token);

            TempData["Success"] = "Result updated successfully.";
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to update the result.";
        }

        return RedirectToAction(
            nameof(Manage),
            new { eventId = model.EventId });
    }
}
