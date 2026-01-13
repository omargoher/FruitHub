using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FruitHub.Infrastructure.Services;
public class ImageService : IImageService
{

    public ImageService()
    {
    }

    public Task<string> ResolveImageAsync(string folder, string fileName)
    {
        if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(fileName))
            throw new InvalidRequestException("Invalid request");

        if (folder.Contains("..") || fileName.Contains(".."))
            throw new InvalidRequestException("Invalid request");

        var root = Directory.GetCurrentDirectory();
        var path = Path.Combine(root, "storage", folder, fileName);

        if (!File.Exists(path))
            throw new NotFoundException($"image");

        return Task.FromResult(path);
    }
    
    public async Task<string> SaveAsync(ImageDto image, string folder)
    {
        using var img = Image.Load(image.Content);
        // resize image with the original aspect ratio and the height or width not exceeds 800
        img.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size (800, 800),
            Mode = ResizeMode.Max,
        }));
        
        var fileName = $"{Guid.NewGuid():N}.webp";
        var path = Path.Combine("storage", folder, fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        using var fs = File.Create(path);
        await img.SaveAsWebpAsync(fs);

        return $"/images/{folder}/{fileName}";
    }
    
    // Delete file is fast operation so not has File.DeleteAsync()
    // So this method not Async
    // But i name it with this becuse implement the IImageInterface and another cases Delete will be Async
    public Task DeleteAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Task.CompletedTask;

        // imageUrl: /images/products/abc.webp
        // we want: storage/products/abc.webp
        var relativePath = imageUrl
            .Replace("/images/", string.Empty)
            .TrimStart('/');

        if (relativePath.Contains(".."))
            throw new InvalidRequestException("Invalid image path");
        
        var physicalPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "storage",
            relativePath
        );

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }
}
