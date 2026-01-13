
using FruitHub.ApplicationCore.DTOs.Product;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IImageService
{
    Task<string> ResolveImageAsync(string folder, string fileName);
    Task<string> SaveAsync(ImageDto image, string folder);

    Task DeleteAsync(string imageUrl);
}