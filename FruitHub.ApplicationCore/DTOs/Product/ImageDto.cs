namespace FruitHub.ApplicationCore.DTOs.Product;

public class ImageDto
{
    public Stream Content { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Length { get; set; }
}