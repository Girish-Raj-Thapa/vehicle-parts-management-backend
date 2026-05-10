using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Customer;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin,Staff")]
public class CustomersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await userService.GetCustomerDetailsAsync(id);
        return result.Success
            ? Ok(result)
            : NotFound(ApiResponse<CustomerDetailsDto>.FailureResponse(result.Message, result.Errors));
    }


    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(string id)
    {
        var result = await userService.GetCustomerHistoryAsync(id);
        return result.Success
            ? Ok(result)
            : NotFound(ApiResponse<CustomerHistoryDto>.FailureResponse(result.Message, result.Errors));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] CustomerSearchQueryDto dto)
    {
        var result = await userService.SearchCustomersAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("me/purchases")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyPurchases(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? paymentStatus = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await userService.GetMyPurchaseHistoryAsync(
            userId,
            page,
            pageSize,
            paymentStatus,
            fromDate,
            toDate);

        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("me/purchases/{saleId:int}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyPurchaseDetail(int saleId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await userService.GetMyPurchaseDetailAsync(userId, saleId);
        return result.Success ? Ok(result) : NotFound(result);
    }


    [HttpGet("me/service-records")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyServiceRecords(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? vehicleId = null)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await userService.GetMyServiceRecordsAsync(userId, page, pageSize, vehicleId);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("me/service-records/{serviceRecordId:int}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyServiceRecordDetail(int serviceRecordId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await userService.GetMyServiceRecordDetailAsync(userId, serviceRecordId);
        return result.Success ? Ok(result) : NotFound(result);
    }


    [HttpGet("me/payments")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await userService.GetMyPaymentHistoryAsync(userId, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("me/appointments")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyAppointments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await userService.GetMyAppointmentHistoryAsync(userId, page, pageSize, status);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("me/history-summary")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyHistorySummary()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await userService.GetMyHistorySummaryAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("{id}/loyalty")]
    public async Task<IActionResult> GetLoyalty(string id)
    {
        var result = await userService.GetCustomerLoyaltyAsync(id);
        return result.Success
            ? Ok(result)
            : NotFound(ApiResponse<CustomerLoyaltyDto>.FailureResponse(result.Message, result.Errors));
    }
}
