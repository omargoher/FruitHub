using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Services;
using Moq;
using Xunit;

namespace FruitHub.Tests;

public class UserFavoritesServiceTest
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IUserFavoritesRepository> _userFavoritesRepo;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IProductRepository> _productRepo;

    public UserFavoritesServiceTest()
    {
        _uow = new Mock<IUnitOfWork>();
        _userFavoritesRepo = new Mock<IUserFavoritesRepository>();
        _userRepo = new Mock<IUserRepository>();
        _productRepo = new Mock<IProductRepository>();

        _uow.Setup(x => x.UserFavorites)
            .Returns(_userFavoritesRepo.Object);

        _uow.Setup(x => x.User)
            .Returns(_userRepo.Object);

        _uow.Setup(x => x.Product)
            .Returns(_productRepo.Object);
    }

    private UserFavoritesService CreateSut()
    {
        return new UserFavoritesService(_uow.Object);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnUserFavoriteProducts()
    {
        // Arrange
        var userId = 1;

        var products = new List<ProductResponseDto>
        {
            new() { Id = 1, Name = "Apple" },
            new() { Id = 2, Name = "Banana" }
        };

        _userFavoritesRepo
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(products);

        var sut = CreateSut();

        // Act
        var result = await sut.GetAllAsync(userId);

        // Assert
        Assert.Equal(products, result);
    }
    
    [Fact]
    public async Task AddAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(false);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.AddAsync(1, 10));
    }

    [Fact]
    public async Task AddAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(true);

        _productRepo.Setup(x => x.IsExistAsync(10))
            .ReturnsAsync(false);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.AddAsync(1, 10));
    }

    [Fact]
    public async Task AddAsync_WhenFavoriteAlreadyExists_ShouldReturnWithoutSaving()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(true);

        _productRepo.Setup(x => x.IsExistAsync(10))
            .ReturnsAsync(true);

        _userFavoritesRepo
            .Setup(x => x.IsExistAsync(1, 10))
            .ReturnsAsync(true);

        var sut = CreateSut();

        // Act
        await sut.AddAsync(1, 10);

        // Assert
        _userFavoritesRepo.Verify(
            x => x.Add(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _uow.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task AddAsync_WhenValid_ShouldAddFavorite()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1))
            .ReturnsAsync(true);

        _productRepo.Setup(x => x.IsExistAsync(10))
            .ReturnsAsync(true);

        _userFavoritesRepo
            .Setup(x => x.IsExistAsync(1, 10))
            .ReturnsAsync(false);

        _userFavoritesRepo
            .Setup(x => x.Add(1, 10));

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.AddAsync(1, 10);

        // Assert
        _userFavoritesRepo.Verify(
            x => x.Add(1, 10),
            Times.Once);

        _uow.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WhenFavoriteDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _userFavoritesRepo
            .Setup(x => x.IsExistAsync(1, 10))
            .ReturnsAsync(false);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.RemoveAsync(1, 10));
    }

    [Fact]
    public async Task RemoveAsync_WhenFavoriteExists_ShouldRemoveAndSaveChanges()
    {
        // Arrange
        _userFavoritesRepo
            .Setup(x => x.IsExistAsync(1, 10))
            .ReturnsAsync(true);

        _userFavoritesRepo
            .Setup(x => x.Remove(1, 10));

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.RemoveAsync(1, 10);

        // Assert
        _userFavoritesRepo.Verify(
            x => x.Remove(1, 10),
            Times.Once);

        _uow.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }
}
