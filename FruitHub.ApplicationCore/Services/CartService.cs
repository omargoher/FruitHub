using FruitHub.ApplicationCore.DTOs.Cart;
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

    public async Task<IReadOnlyList<CartResponseDto>> GetItemsAsync(string identityUserId)
    {
        var user = await _userRepo.GetByIdentityUserIdAsync(identityUserId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return await _cartRepo.GetWithCartItemsAsync(user.Id);
    }

    public async Task AddItemAsync(string identityUserId, int productId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        var user = await _userRepo.GetByIdentityUserIdWithCartAsync(identityUserId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var product = await _productRepo.GetByIdAsync(productId);

        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
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
                throw new InvalidOperationException($"Can set only {product.Stock}");
            }

            existItem.Quantity = newQuantity;
        }
        else
        {
            if (product.Stock < quantity)
            {
                throw new InvalidOperationException($"Can set only {product.Stock}");
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

    public async Task UpdateQuantityAsync(string identityUserId, int productId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        var user = await _userRepo.GetByIdentityUserIdWithCartAsync(identityUserId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var product = await _productRepo.GetByIdAsync(productId);

        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        if (product.Stock < quantity)
        {
            throw new InvalidOperationException($"Can set only {product.Stock}");
        }

        if (user.Cart == null)
        {
            throw new KeyNotFoundException("Cart not found");
        }

        var existItem = user.Cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

        if (existItem == null)
        {
            throw new KeyNotFoundException("Item not found in cart");
        }
        existItem.Quantity = quantity;

        await _uow.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(string identityUserId, int productId)
    {
        var user = await _userRepo.GetByIdentityUserIdWithCartAsync(identityUserId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        if (user.Cart == null)
        {
            throw new KeyNotFoundException("Cart not found");
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