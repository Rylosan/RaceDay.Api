using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AuthController : Controller
{
    private readonly RaceDayApiService _api;

    public AuthController(RaceDayApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _api.PostAsync<LoginModel, AuthResponseModel>(
                "api/Auth/login", model);

            if (response == null || string.IsNullOrWhiteSpace(response.Token))
            {
                ModelState.AddModelError("", "Login failed.");
                return View(model);
            }

            HttpContext.Session.SetString("Token", response.Token);
            HttpContext.Session.SetString("Email", response.Email);
            HttpContext.Session.SetString("Role", response.Role);

            return RedirectToAction("Index", "Dashboard");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _api.PostAsync<RegisterModel, AuthResponseModel>(
                "api/Auth/register", model);

            if (response == null || string.IsNullOrWhiteSpace(response.Token))
            {
                ModelState.AddModelError("", "Registration failed.");
                return View(model);
            }

            HttpContext.Session.SetString("Token", response.Token);
            HttpContext.Session.SetString("Email", response.Email);
            HttpContext.Session.SetString("Role", response.Role);

            return RedirectToAction("Index", "Dashboard");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Registration failed. Check your details or email address.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}