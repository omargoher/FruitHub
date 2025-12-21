using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

public class PasswordResetsController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}