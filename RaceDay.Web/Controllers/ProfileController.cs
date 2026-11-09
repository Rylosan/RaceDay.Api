using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ProfileController : Controller
{
    private readonly RaceDayApiService _apiService;

    public ProfileController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    // Display the logged-in participant's profile
    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        try
        {
            var profile = await _apiService.GetAsync<ProfileModel>(
                "api/Profile/me", token);

            if (profile == null)
            {
                TempData["Error"] = "Could not load your profile.";
                return View(new ProfileModel());
            }

            return View(profile);
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"API Error: {ex.Message}";
            return View(new ProfileModel());
        }
    }

    // Save changes to the participant's profile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileModel model)
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View("Index", model);

        try
        {
            var updatedProfile =
                await _apiService.PutAsync<ProfileModel, ProfileModel>(
                    "api/Profile/me", model, token);

            if (updatedProfile == null)
            {
                TempData["Error"] = "Could not update your profile.";
                return View("Index", model);
            }

            TempData["Success"] = "Your profile has been updated.";

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to update your profile. Please try again.";
            return View("Index", model);
        }
    }
}