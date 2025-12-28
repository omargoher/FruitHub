
namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IImageService
{
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType);

    Task DeleteAsync(string imagePath);
}