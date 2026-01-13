using FruitHub.ApplicationCore.DTOs.Product;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IImageValidator
{
    void Validate(ImageDto image);
}