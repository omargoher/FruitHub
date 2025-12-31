using System.Security.Claims;
using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllOrdersAsync([FromQuery] OrderQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (userId == null)
        {
            return Unauthorized();
        }

        IReadOnlyList<OrderResponseDto> orders;
        
        if (role == "Admin")
        {
            orders = await _orderService.GetAllAsync(query);  
        }
        else
        {
            orders = await _orderService.GetAllForUserAsync(userId, query);  
        }
        
        if (!orders.Any())
        {
            return NoContent();
        }
        return Ok(orders);
    }
    
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrderAsync(int orderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (userId == null)
        {
            return Unauthorized();
        }

        var order = await _orderService.GetByIdAsync(userId, role, orderId);

        if (order == null)
        {
            return NoContent();
        }
        return Ok(order);
    }
    
    [HttpPost]
    public async Task<IActionResult> CheckOutAsync(CheckoutDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
    
        await _orderService.Checkout(userId, dto);
        
        return NoContent();
    }
    
    [HttpPost("{orderId:int}")]
    public async Task<IActionResult> ChangeOrderStatusAsync(int orderId, ChangeOrderStatusDto dto)
    {
        if (dto.IsShipped.HasValue)
        {
            return NoContent();
        }
        await _orderService.MarkOrderIsAShipped(orderId);
        
        return NoContent();
    }

}