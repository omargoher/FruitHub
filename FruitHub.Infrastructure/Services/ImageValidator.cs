using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces.Services;

namespace FruitHub.Infrastructure.Services;

public class ImageValidator : IImageValidator
{
    private static readonly HashSet<string> AllowedTypes =
    [
        "image/jpeg",
        "image/png",
    ];
    
    public void Validate(ImageDto image)
    {
        if (image.Length == 0)
            throw new InvalidRequestException("Image is required");

        if (image.Length > 5 * 1024 * 1024)
            throw new InvalidRequestException("Image size must be less than 5MB");

        if (!AllowedTypes.Contains(image.ContentType))
            throw new InvalidRequestException("Unsupported image type");

        // validate image base on the prefix bytes not a content type 
        ValidateSignature(image.Content);
    }
    
    private static void ValidateSignature(Stream stream)
    {
        Span<byte> header = stackalloc byte[8];
        stream.Read(header);
        stream.Position = 0;

        // JPEG => JPEG files always start with 0xFF 0xD8
        if (header[0] == 0xFF && header[1] == 0xD8)
            return;

        // PNG => PNG files always start with 0x89 0x50
        if (header[0] == 0x89 && header[1] == 0x50)
            return;

        throw new InvalidRequestException("Invalid image file image should be JPEG or PNG");
    }
}