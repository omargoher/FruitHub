using FruitHub.ApplicationCore.Options;
using FruitHub.ApplicationCore.Interfaces.Services;

namespace FruitHub.Infrastructure.Services;
public class ImageService : IImageService
{
    private readonly ImageStorageOptions _options;

    public ImageService(ImageStorageOptions options)
    {
        _options = options;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType)
    {
        if (!contentType.StartsWith("image/"))
            throw new InvalidOperationException("Invalid image type");

        var extension = Path.GetExtension(fileName);
        var newName = $"{Guid.NewGuid()}{extension}";

        var fullPath = Path.Combine(_options.RootPath, newName);
        Directory.CreateDirectory(_options.RootPath);

        using var fs = new FileStream(fullPath, FileMode.Create);
        await content.CopyToAsync(fs);

        return $"{_options.PublicBasePath}/{newName}";
    }

    public Task DeleteAsync(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return Task.CompletedTask;

        // imagePath example: /images/8f3a2c.jpg
        var fileName = Path.GetFileName(imagePath);

        var fullPath = Path.Combine(_options.RootPath, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
