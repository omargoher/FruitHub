using FruitHub.ApplicationCore.DTOs.Category;
using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Services;
using FruitHub.Infrastructure.Services;
using Moq;

namespace FruitHub.Tests;

public class ProductServiceTest
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IImageService> _imageService;
    
    public ProductServiceTest()
    {
        _uow = new Mock<IUnitOfWork>();
        _imageService = new Mock<IImageService>();
    }
    
    private ProductService CreateSut()
    {
        return new ProductService(
            _uow.Object,
            _imageService.Object
        );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnListOfProduct()
    {
        // Arrange 
        _uow.Setup(x => x.Product.GetProductsAsync(It.IsAny<ProductQuery>()))
            .ReturnsAsync(new List<ProductResponseDto>
            {
                new ProductResponseDto
                {
                    Id = 1,
                    Name = "p1",
                    Price = 55.0m
                },
                new ProductResponseDto
                {
                    Id = 2,
                    Name = "p2",
                    Price = 55.0m
                }
            });
        var sut = CreateSut();
        
        // Act
        var result = await sut.GetAllAsync(new ProductQuery());
        
        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFoundProduct_ShouldReturnProduct()
    {
        // Arrange 
        _uow.Setup(x => x.Product.GetProductByIdWithCategoryAsync(1))
            .ReturnsAsync(new SingleProductResponseDto
            {
                Id = 1,
                Name = "p1",
                Price = 55.0m
            });
        var sut = CreateSut();
        
        // Act
        var result = await sut.GetByIdAsync(1);
        
        // Assert
        Assert.Equal("p1", result.Name);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFoundProduct_ShouldReturnNull()
    {
        // Arrange 
        _uow.Setup(x => x.Product.GetProductByIdWithCategoryAsync(1))
            .ReturnsAsync((SingleProductResponseDto?)null);
        var sut = CreateSut();
        
        // Act
        var result = await sut.GetByIdAsync(1);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnListOfProduct()
    {
        // Arrange 
        _uow.Setup(x => x.Product.GetByCategoryAsync(It.IsAny<int>(), It.IsAny<ProductQuery>()))
            .ReturnsAsync(new List<ProductResponseDto>
            {
                new ProductResponseDto
                {
                    Id = 1,
                    Name = "p1",
                    Price = 55.0m
                },
                new ProductResponseDto
                {
                    Id = 2,
                    Name = "p2",
                    Price = 55.0m
                }
            });
        var sut = CreateSut();
        
        // Act
        var result = await sut.GetByCategoryAsync(1, new ProductQuery());
        
        // Assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task CreateAsync_WhenDtoIsNull_ThrowArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        
        // Act
        var result = () => sut.CreateAsync(null, new ImageDto());
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(result);
    }
    
    [Fact]
    public async Task CreateAsync_WhenImageDtoIsNull_ThrowArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        
        // Act
        var result = () => sut.CreateAsync(new CreateProductDto(), null);
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(result);
    }
    
    [Fact]
    public async Task CreateAsync_WhenValid_AddProduct()
    {
        // Arrange
        var imagePath = "/img/test.png";
        _imageService.Setup(x =>
                x.SaveAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
            .ReturnsAsync(imagePath);
        _uow.Setup(x => x.Product.Add(It.IsAny<Product>()));
        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        var sut = CreateSut();
        var dto = new CreateProductDto
        {
            Name = "p1",
            Price = 55.2m,
            Calories = 55,
            Description = "plaplapla",
            Organic = true,
            ExpirationPeriodByDays = 5,
            Stock = 10,
            CategoryId = 1,
        };
        
        // Act
        await sut.CreateAsync(dto, new ImageDto());
        
        // Assert
        _imageService.Verify(x => x.SaveAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
        _uow.Verify(x => x.Product.Add(It.IsAny<Product>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenProductNotFound_ThrowKeyNotFoundException()
    {
        // Arrange
        _uow.Setup(x => x.Product.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);
        var sut = CreateSut();
        
        // Act
        var result = () => sut.UpdateAsync(new UpdateProductDto(), new ImageDto());
        
        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenImageDtoIsNullAndFoundProduct_UpdateProductWithoutUpdateImage()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "p1",
            Price = 55.2m,
            Calories = 55,
            Description = "plaplapla",
            Organic = true,
            ExpirationPeriodByDays = 5,
            Stock = 10,
            CategoryId = 1,
        };
        _uow.Setup(x => x.Product.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(product);
        _uow.Setup(x => x.Product.Update(It.IsAny<Product>()));
        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        var sut = CreateSut();
        var dto = new UpdateProductDto
        {
            Id = 1,
            Name = "p2",
            Price = 66.0m,
        };
        
        // Act
        await sut.UpdateAsync(dto, null);
        
        // Assert
        Assert.Equal(dto.Name, product.Name);
        Assert.Equal(dto.Price, product.Price);
        _uow.Verify(x => x.Product.Update(It.IsAny<Product>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenImageDtoIsNotNullAndFoundProduct_UpdateProductAndSaveNewImageAndDeleteOldImage()
    {
        // Arrange
        var oldImagePath = "/img/old.png";
        var newImagePath = "/img/new.png";
        var product = new Product
        {
            Id = 1,
            Name = "p1",
            Price = 55.2m,
            Calories = 55,
            Description = "plaplapla",
            Organic = true,
            ExpirationPeriodByDays = 5,
            Stock = 10,
            CategoryId = 1,
            ImagePath = oldImagePath,
        };
        
        _imageService.Setup(x =>
                x.SaveAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
            .ReturnsAsync(newImagePath);
        
        _uow.Setup(x => x.Product.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(product);
        
        _uow.Setup(x => x.Product.Update(It.IsAny<Product>()));
        
        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        _imageService.Setup(x => x.DeleteAsync(It.IsAny<string>()));
        
        var sut = CreateSut();
        var dto = new UpdateProductDto
        {
            Id = 1,
            Name = "p2",
            Price = 66.0m,
        };
        
        // Act
        await sut.UpdateAsync(dto, new ImageDto());
        
        // Assert
        Assert.Equal(dto.Name, product.Name);
        Assert.Equal(dto.Price, product.Price);
        Assert.Equal(newImagePath, product.ImagePath);
        _imageService.Verify(x => x.SaveAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
        _imageService.Verify(x => x.DeleteAsync(It.IsAny<string>()));
        _uow.Verify(x => x.Product.Update(It.IsAny<Product>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    [Fact]
    public async Task DeleteAsync_WhenProductNotFound_ThrowKeyNotFoundException()
    {
        // Arrange
        _uow.Setup(x => x.Product.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);
        var sut = CreateSut();
        
        // Act
        var result = () => sut.DeleteAsync(1);
        
        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(result);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenValid_DeleteProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 2,
            Name = "p2",
            Price = 55.0m
        };
        _uow.Setup(x => x.Product.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(product);
        _uow.Setup(x => x.Product.Remove(It.IsAny<Product>()));
        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        _imageService.Setup(x => x.DeleteAsync(It.IsAny<string>()));
        var sut = CreateSut();
        
        // Act
        await sut.DeleteAsync(1);
        
        // Assert
        _imageService.Verify(x => x.DeleteAsync(It.IsAny<string>()));
        _uow.Verify(x => x.Product.Remove(It.IsAny<Product>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}