
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EnrollmentsController : Controller
{
    private readonly RaceDayApiService _api;

    public EnrollmentsController(RaceDayApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("Role") != "Participant")
            return Forbid();

        try
        {
            var enrollments = await _api.GetAsync<List<EnrollmentModel>>(
                "api/Enrollments/me", token);

            return View(enrollments ?? new List<EnrollmentModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "Unable to retrieve your enrolments.";
            return View(new List<EnrollmentModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(int eventId)
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("Role") != "Participant")
            return Forbid();

        try
        {
            var categories = await _api.GetAsync<List<CategoryModel>>(
                $"api/events/{eventId}/categories", token);

            ViewBag.EventId = eventId;
            ViewBag.Categories = categories ?? new List<CategoryModel>();

            return View();
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to load event categories.";
            return RedirectToAction("Index", "Events");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEnrollmentModel model)
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("Role") != "Participant")
            return Forbid();

        if (!ModelState.IsValid)
            return RedirectToAction("Create", new { eventId = model.EventId });

        try
        {
            await _api.PostAsync<CreateEnrollmentModel, System.Text.Json.JsonElement>(
                "api/Enrollments", model, token);

            TempData["Success"] = "You have successfully enrolled!";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Enrolment failed. You may already be enrolled or the selected category may be invalid.";
            return RedirectToAction("Create", new { eventId = model.EventId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (HttpContext.Session.GetString("Role") != "Participant")
            return Forbid();

        try
        {
            await _api.DeleteAsync($"api/Enrollments/{id}", token);
            TempData["Success"] = "Enrolment cancelled successfully.";
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to cancel this enrolment.";
        }

        return RedirectToAction(nameof(Index));
    }
}
