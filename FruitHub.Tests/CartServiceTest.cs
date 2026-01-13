using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Services;
using Moq;
using Xunit;

namespace FruitHub.Tests;

public class CartServiceTest
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IProductRepository> _productRepo;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<ICartRepository> _cartRepo;

    public CartServiceTest()
    {
        _uow = new Mock<IUnitOfWork>();
        _productRepo = new Mock<IProductRepository>();
        _userRepo = new Mock<IUserRepository>();
        _cartRepo = new Mock<ICartRepository>();

        _uow.Setup(x => x.Product).Returns(_productRepo.Object);
        _uow.Setup(x => x.User).Returns(_userRepo.Object);
        _uow.Setup(x => x.Cart).Returns(_cartRepo.Object);
    }

    private CartService CreateSut()
        => new CartService(_uow.Object);
    
    [Fact]
    public async Task GetAllItemsAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(false);
        
        var sut = CreateSut();

        // ACT & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.GetAllItemsAsync(1));
    }

    [Fact]
    public async Task GetAllItemsAsync_WhenUserExists_ShouldReturnCart()
    {
        // Arrange
        var cart = new CartResponseDto 
        {
            Items = new List<CartItemResponseDto>
            {
                new() { ProductId = 1, Quantity = 2 }
            }
        };

        _userRepo.Setup(x => x.IsExistAsync(1)).ReturnsAsync(true);
        _cartRepo.Setup(x => x.GetByUserIdWithCartItemsAsync(1))
                 .ReturnsAsync(cart);

        var sut = CreateSut();

        // ACT
        var result = await sut.GetAllItemsAsync(1);

        // Assert
        Assert.Equal(cart, result);
    }

    [Fact]
    public async Task AddItemAsync_WhenQuantityInvalid_ShouldThrowInvalidRequestException()
    {
        // Arrange
        var sut = CreateSut();

        // ACT & Assert
        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            sut.AddItemAsync(1, 1, 0));
    }

    [Fact]
    public async Task AddItemAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.GetByIdWithCartAsync(1))
                 .ReturnsAsync((User?)null);

        var sut = CreateSut();

        // ACT & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.AddItemAsync(1, 1, 2));
    }

    [Fact]
    public async Task AddItemAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.GetByIdWithCartAsync(1))
                 .ReturnsAsync(new User { Id = 1 });

        _productRepo.Setup(x => x.GetByIdAsync(1))
                    .ReturnsAsync((Product?)null);

        var sut = CreateSut();

        // ACT & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.AddItemAsync(1, 1, 2));
    }

    [Fact]
    public async Task AddItemAsync_WhenNewItemAndStockEnough_ShouldAddItem()
    {
        // Arrange
        var user = new User { Id = 1 };
        var product = new Product { Id = 1, Stock = 10 };

        _userRepo.Setup(x => x.GetByIdWithCartAsync(1)).ReturnsAsync(user);
        _productRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        // ACT
        await sut.AddItemAsync(1, 1, 3);

        // Assert
        Assert.Single(user.Cart!.Items);
        Assert.Equal(3, user.Cart.Items[0].Quantity);
    }

    [Fact]
    public async Task AddItemAsync_WhenItemExists_ShouldIncreaseQuantity()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Cart = new Cart
            {
                Items = new List<CartItem>
                {
                    new() { ProductId = 1, Quantity = 2 }
                }
            }
        };

        var product = new Product { Id = 1, Stock = 10 };

        _userRepo.Setup(x => x.GetByIdWithCartAsync(1)).ReturnsAsync(user);
        _productRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        // ACT
        await sut.AddItemAsync(1, 1, 3);

        // Assert
        Assert.Equal(5, user.Cart.Items[0].Quantity);
    }

   
    [Fact]
    public async Task UpdateQuantityAsync_WhenQuantityInvalid_ShouldThrowInvalidRequestException()
    {
        // Arrange
        var sut = CreateSut();
        
        // ACT & Assert
        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            sut.UpdateQuantityAsync(1, 1, 0));
    }

    [Fact]
    public async Task UpdateQuantityAsync_WhenCartNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepo.Setup(x => x.GetByIdWithCartAsync(1))
                 .ReturnsAsync(new User { Id = 1 });

        _productRepo.Setup(x => x.GetByIdAsync(1))
                    .ReturnsAsync(new Product { Id = 1, Stock = 10 });

        var sut = CreateSut();

        // ACT & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateQuantityAsync(1, 1, 3));
    }

    [Fact]
    public async Task UpdateQuantityAsync_WhenItemExists_ShouldUpdateQuantity()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Cart = new Cart
            {
                Items = new List<CartItem>
                {
                    new() { ProductId = 1, Quantity = 2 }
                }
            }
        };

        _userRepo.Setup(x => x.GetByIdWithCartAsync(1)).ReturnsAsync(user);
        _productRepo.Setup(x => x.GetByIdAsync(1))
                    .ReturnsAsync(new Product { Id = 1, Stock = 10 });

        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        // ACT
        await sut.UpdateQuantityAsync(1, 1, 5);

        // Assert
        Assert.Equal(5, user.Cart.Items[0].Quantity);
    }

    
    [Fact]
    public async Task RemoveItemAsync_WhenItemNotFound_ShouldReturnWithoutSaving()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Cart = new Cart { Items = new List<CartItem>() }
        };

        _userRepo.Setup(x => x.GetByIdWithCartAsync(1)).ReturnsAsync(user);

        var sut = CreateSut();

        // ACT
        await sut.RemoveItemAsync(1, 1);

        // Assert
        _uow.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenItemExists_ShouldRemoveAndSave()
    {
        // Arrange
        var item = new CartItem { ProductId = 1 };
        var cart = new Cart { Items = new List<CartItem> { item } };
        item.Cart = cart;

        var user = new User { Id = 1, Cart = cart };

        _userRepo.Setup(x => x.GetByIdWithCartAsync(1)).ReturnsAsync(user);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = CreateSut();
        
        // ACT
        await sut.RemoveItemAsync(1, 1);

        // Assert
        Assert.Empty(cart.Items);
    }
}
