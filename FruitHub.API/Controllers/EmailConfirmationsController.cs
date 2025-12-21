using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

public class EmailConfirmationsController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}