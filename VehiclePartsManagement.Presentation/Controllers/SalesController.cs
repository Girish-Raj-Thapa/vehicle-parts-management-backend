using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Sale;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize]
public class SalesController(ISaleService saleService) : ControllerBase
{
    [HttpGet("all")]
    public async Task<IActionResult> GetAllSales()
    {
        var response = await saleService.GetAllSalesAsync();
        return Ok(response);
    }

    [HttpPost("{saleId}/send-email")]
    public async Task<IActionResult> SendEmail(int saleId)
    {
        var response = await saleService.SendInvoiceEmailAsync(saleId);
        return Ok(response);
    }
    /// <summary>
    /// Creates a sale invoice and records part sale transaction by staff/admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateSaleDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await saleService.CreateAsync(dto, userId);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Creates an online parts-only sale for the authenticated customer.
    /// </summary>
    [HttpPost("online-checkout")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateOnline([FromBody] CreateOnlineSaleDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await saleService.CreateOnlineAsync(dto, userId);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    [HttpGet("my-online-orders")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyOnlineOrders([FromQuery] SaleListQueryDto query)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        query.Mine = true;
        query.SaleChannel = "OnlineParts";
        var result = await saleService.GetPagedAsync(query, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("my-online-orders/{saleId:int}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyOnlineOrderById(int saleId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await saleService.GetMyOnlineOrderByIdAsync(saleId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("my-online-orders/{saleId:int}/invoice")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyOnlineOrderInvoice(int saleId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await saleService.GetMyOnlineOrderInvoiceAsync(saleId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("my-online-orders/recent-parts")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyRecentPurchasedParts([FromQuery] int limit = 10)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await saleService.GetRecentPurchasedPartsAsync(userId, limit);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// Returns paged sales with role-based customer scoping.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetPaged([FromQuery] SaleListQueryDto query)
    {
        var isCustomer = User.IsInRole("Customer");
        var userId = isCustomer || query.Mine == true ? GetCurrentUserId() : null;
        if ((isCustomer || query.Mine == true) && string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        // Customer listing is always constrained to own sales regardless of incoming query.
        if (isCustomer)
        {
            query.Mine = true;
        }

        var result = await saleService.GetPagedAsync(query, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// Returns one sale detail; customer access is constrained to own records.
    /// </summary>
    [HttpGet("{saleId:int}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetById(int saleId)
    {
        var scopedUserId = User.IsInRole("Customer") ? GetCurrentUserId() : null;
        if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(scopedUserId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await saleService.GetByIdAsync(saleId, scopedUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Returns invoice payload for one sale; customer access is constrained to own records.
    /// </summary>
    [HttpGet("{saleId:int}/invoice")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetInvoice(int saleId)
    {
        var scopedUserId = User.IsInRole("Customer") ? GetCurrentUserId() : null;
        if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(scopedUserId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await saleService.GetInvoiceAsync(saleId, scopedUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }


    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
