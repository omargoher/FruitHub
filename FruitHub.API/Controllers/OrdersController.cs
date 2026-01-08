using System.Security.Claims;
using FruitHub.API.Extensions;
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
    public async Task<IActionResult> GetAllAsync([FromQuery] OrderQuery query)
    {
        IReadOnlyList<OrderResponseDto> orders;
        
        if (IsAdmin())
        {
            orders = await _orderService.GetAllAsync(query);  
        }
        else
        {
            var userId = ClaimsPrincipalExtensions.GetUserId(User);
            orders = await _orderService.GetAllForUserAsync(userId, query);  
        }
        
        return Ok(orders);
    }
    
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetByIdAsync(int orderId)
    {
        OrderResponseDto? order;

        if (IsAdmin())
        {
            order = await _orderService.GetByIdAsync(orderId);
        }
        else
        {
            var userId = ClaimsPrincipalExtensions.GetUserId(User);
            order = await _orderService.GetByIdAsync(userId, orderId);  
        }

        return Ok(order);
    }
    
    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<IActionResult> CheckoutAsync(CheckoutDto dto)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
    
        await _orderService.CheckoutAsync(userId, dto);
        
        return NoContent();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{orderId:int}/status")]
    public async Task<IActionResult> ChangeOrderStatusAsync(int orderId, ChangeOrderStatusDto dto)
    {
        await _orderService.ChangeOrderStatusAsync(orderId, dto);
        
        return NoContent();
    }
    
    [HttpDelete("{orderId:int}")]
    public async Task<IActionResult> CancelOrderAsync(int orderId)
    {
        if (IsAdmin())
        {
            await _orderService.CancelOrderAsync(orderId);
        }
        else
        {
            var userId = ClaimsPrincipalExtensions.GetUserId(User);
            await _orderService.CancelOrderAsync(userId, orderId);  
        }
        
        return NoContent();
    }

    private bool IsAdmin() =>
        User.IsInRole("Admin");
    
}