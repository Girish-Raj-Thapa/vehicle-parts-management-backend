using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Cart;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "Customer")]
public class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyCart()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await cartService.GetMyCartAsync(userId);
        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await cartService.AddItemAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await cartService.UpdateItemAsync(userId, cartItemId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await cartService.RemoveItemAsync(userId, cartItemId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await cartService.ClearAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await cartService.CheckoutAsync(userId);
        return result.Success ? StatusCode(StatusCodes.Status201Created, result) : BadRequest(result);
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
