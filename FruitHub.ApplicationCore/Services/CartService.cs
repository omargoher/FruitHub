using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _uow;
    private readonly IProductRepository _productRepo;
    private readonly IUserRepository _userRepo;
    private readonly ICartRepository _cartRepo;

    public CartService(IUnitOfWork uow)
    {
        _uow = uow;
        _productRepo = uow.Product;
        _userRepo = uow.User;
        _cartRepo = uow.Cart;
    }

    public async Task<IReadOnlyList<CartResponseDto>> GetAllItemsAsync(int userId)
    {
        if (!await _userRepo.IsExistAsync(userId))
        {
            throw new NotFoundException("User");
        }

        return await _cartRepo.GetByUserIdWithCartItemsAsync(userId);
    }

    public async Task AddItemAsync(int userId, int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidRequestException("Quantity must be greater than zero");
        }

        var user = await _userRepo.GetByIdWithCartAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User");
        }

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null)
        {
            throw new NotFoundException("Product");
        }

        user.Cart ??= new Cart
        {
            UserId = user.Id,
            Items = new List<CartItem>()
        };

        var existItem = user.Cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

        if (existItem != null)
        {
            var newQuantity = existItem.Quantity + quantity;
            if (product.Stock < newQuantity)
            {
                throw new InvalidRequestException($"Can set only {product.Stock} and you set {newQuantity}");
            }

            existItem.Quantity = newQuantity;
        }
        else
        {
            if (product.Stock < quantity)
            {
                throw new InvalidRequestException($"Can set only {product.Stock} and you try set {quantity}");
            }

            var item = new CartItem
            {
                ProductId = productId,
                Quantity = quantity
            };
            user.Cart.Items.Add(item);
        }

        await _uow.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidRequestException("Quantity must be greater than zero");

        var user = await _userRepo.GetByIdWithCartAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User");
        }

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null)
        {
            throw new NotFoundException("Product");
        }

        if (product.Stock < quantity)
        {
            throw new InvalidRequestException($"Can set only {product.Stock} and you try set {quantity}");
        }

        if (user.Cart == null)
        {
            throw new NotFoundException("Cart");
        }

        var existItem = user.Cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

        if (existItem == null)
        {
            throw new NotFoundException($"Product");
        }
        existItem.Quantity = quantity;

        await _uow.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(int userId, int productId)
    {
        var user = await _userRepo.GetByIdWithCartAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User");
        }
        
        if (user.Cart == null)
        {
            throw new NotFoundException("Cart");
        }
        
        var existItem = user.Cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

        if (existItem == null)
        {
            return;
        }
        
        existItem.Cart.Items.Remove(existItem);
        await _uow.SaveChangesAsync();
    }
}