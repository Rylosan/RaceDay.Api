using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        var role = HttpContext.Session.GetString("Role");

        if (role == "Organiser")
        {
            return View("Organiser");
        }

        if (role == "Participant")
        {
            return View("Participant");
        }

        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Auth");
    }

    public IActionResult Organiser()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        return View();
    }

    public IActionResult Participant()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return Forbid();

        return View();
    }
}
