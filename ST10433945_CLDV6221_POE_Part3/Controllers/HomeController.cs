using Microsoft.AspNetCore.Mvc;

// Controller for handling basic site navigation pages
public class HomeController : Controller
{
    // GET: /
    // Loads the homepage
    public IActionResult Index()
    {
        return View();
    }

    // GET: /Home/About
    // Loads the About page with a message about the system
    public IActionResult About()
    {
        ViewData["Message"] = "EventEase Booking System - Your trusted event booking platform.";
        return View();
    }

    // GET: /Home/Contact
    // Loads the Contact page with a message for users
    public IActionResult Contact()
    {
        ViewData["Message"] = "Contact us for support and inquiries.";
        return View();
    }

    // GET: /Home/Privacy
    // Loads the Privacy Policy page
    public IActionResult Privacy()
    {
        return View();
    }
}
