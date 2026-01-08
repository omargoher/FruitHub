using FruitHub.ApplicationCore.DTOs.User;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Services;
using Moq;

namespace FruitHub.Tests;

public class UserServiceTest
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IUserRepository> _userRepo;

    public UserServiceTest()
    {
        _uow = new Mock<IUnitOfWork>();
        _userRepo = new Mock<IUserRepository>();

        _uow.Setup(x => x.User)
            .Returns(_userRepo.Object);
    }

    private UserService CreateSut()
    {
        return new UserService(_uow.Object);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUserProfileDto()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FullName = "Omar Goher",
            Email = "omar@example.com"
        };

        _userRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.FullName, result.FullName);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.GetByIdAsync(1));
    }
    
    [Fact]
    public async Task UpdateAsync_WhenUserExists_ShouldUpdateUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FullName = "Old Name"
        };

        var dto = new UpdateUserDto
        {
            FullName = "New Name"
        };

        _userRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        _userRepo.Setup(x => x.Update(user));

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert
        Assert.Equal(dto.FullName, user.FullName);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            FullName = "New Name"
        };

        _userRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateAsync(1, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenFullNameIsNull_ShouldNotChangeExistingFullName()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FullName = "Existing Name"
        };

        var dto = new UpdateUserDto
        {
            FullName = null
        };

        _userRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        _userRepo.Setup(x => x.Update(user));

        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert
        Assert.Equal("Existing Name", user.FullName);
    }
}
