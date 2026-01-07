using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly  IImageService _imageService;
    
    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }
    
    [HttpGet("{folder}/{fileName}")]
    public async Task<IActionResult> Get(string folder, string fileName)
    {
        var path =  await _imageService.ResolveImageAsync(folder, fileName);

        return PhysicalFile(path, "image/webp");
    }
}