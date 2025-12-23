
namespace FruitHub.Infrastructure.Interfaces;

public interface IImageService
{
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType);

    Task DeleteAsync(string imagePath);
}