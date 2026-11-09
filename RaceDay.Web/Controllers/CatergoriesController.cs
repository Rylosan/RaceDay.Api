using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly RaceDayApiService _api;

    public CategoriesController(RaceDayApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int eventId)
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        try
        {
            var categories = await _api.GetAsync<List<CategoryModel>>(
                $"api/events/{eventId}/categories", token);

            ViewBag.EventId = eventId;
            return View(categories ?? new List<CategoryModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "Unable to retrieve categories.";
            ViewBag.EventId = eventId;
            return View(new List<CategoryModel>());
        }
    }

    [HttpGet]
    public IActionResult Create(int eventId)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        ViewBag.EventId = eventId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int eventId, CreateCategoryModel model)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.EventId = eventId;
            return View(model);
        }

        var token = HttpContext.Session.GetString("Token");

        try
        {
            await _api.PostAsync<CreateCategoryModel, System.Text.Json.JsonElement>(
                $"api/events/{eventId}/categories", model, token);

            TempData["Success"] = "Category created successfully!";
            return RedirectToAction(nameof(Index), new { eventId });
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Could not create category. Check that you own this event.");
            ViewBag.EventId = eventId;
            return View(model);
        }
    }
}