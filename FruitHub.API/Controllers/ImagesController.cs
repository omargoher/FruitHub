using FruitHub.API.Responses;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

/// <summary>
/// Serves stored image files.
/// </summary>
/// <remarks>
/// This controller provides read-only access to image files stored on the server.
///
/// **Security Rules:**
/// - Directory traversal is not allowed.
/// - Only files inside the configured storage directory can be accessed.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Images")]
public class ImagesController : ControllerBase
{
    private readonly  IImageService _imageService;
    
    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }
    
    /// <summary>
    /// Retrieves an image by folder and file name.
    /// </summary>
    /// <remarks>
    /// Returns the image as a physical file response.
    ///
    /// **Path structure:**
    /// `storage/{folder}/{fileName}`
    ///
    /// **Notes:**
    /// - Path traversal (`..`) is blocked.
    /// - Images are served as `image/webp`.
    /// </remarks>
    /// <param name="folder">Folder containing the image.</param>
    /// <param name="fileName">Image file name.</param>
    /// <returns>The requested image file.</returns>
    /// <response code="200">Image retrieved successfully.</response>
    /// <response code="400">Invalid folder or file name.</response>
    /// <response code="404">Image not found.</response>
    [HttpGet("{folder}/{fileName}")]
    [Produces("image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string folder, string fileName)
    {
        var path =  await _imageService.ResolveImageAsync(folder, fileName);
        return PhysicalFile(path, "image/webp");
    }
}