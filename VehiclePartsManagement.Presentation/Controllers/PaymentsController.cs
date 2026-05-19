using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Payment;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost("khalti/initiate")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> InitiateKhaltiPayment([FromBody] KhaltiInitiatePaymentDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await paymentService.InitiateKhaltiPaymentAsync(dto, userId, CanManagePayments());
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("khalti/pay/{saleId}")]
    [AllowAnonymous]
    public async Task<IActionResult> DirectKhaltiPay([FromRoute] int saleId)
    {
        var result = await paymentService.InitiateKhaltiPaymentAsync(
            new KhaltiInitiatePaymentDto { SaleId = saleId },
            userId: string.Empty,
            canManagePayments: true);

        if (result.Success && result.Data is KhaltiInitiateResponseDto paymentData)
        {
            return Redirect(paymentData.PaymentUrl);
        }

        var errorMessage = result.Message ?? "Failed to initiate payment.";
        if (result.Errors != null && result.Errors.Count > 0)
        {
            var errorDetails = string.Join("; ", result.Errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}"));
            errorMessage += $" ({errorDetails})";
        }
        return Redirect($"http://localhost:5173/staff/sales/{saleId}?payment_error={Uri.EscapeDataString(errorMessage)}");
    }


    [HttpPost("khalti/verify")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> VerifyKhaltiPayment([FromBody] KhaltiVerifyPaymentDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await paymentService.VerifyKhaltiPaymentAsync(dto, userId, CanManagePayments());
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("khalti/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> KhaltiCallback(
        [FromQuery] string pidx, 
        [FromQuery] string? status,
        [FromQuery] string? purchase_order_id,
        [FromQuery] int? saleId)
    {
        int actualSaleId = saleId ?? 0;
        if (actualSaleId == 0 && !string.IsNullOrWhiteSpace(purchase_order_id))
        {
            var parts = purchase_order_id.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out var parsedSaleId))
            {
                actualSaleId = parsedSaleId;
            }
        }

        if (string.IsNullOrWhiteSpace(pidx))
        {
            return Redirect("http://localhost:5173/staff/sales?error=pidx_missing");
        }

        var result = await paymentService.VerifyKhaltiPaymentAsync(
            new KhaltiVerifyPaymentDto { Pidx = pidx, SaleId = actualSaleId },
            userId: null,
            canManagePayments: true);

        if (result.Success)
        {
            return Redirect($"http://localhost:5173/staff/sales/{actualSaleId}?payment_success=true");
        }
        
        return Redirect($"http://localhost:5173/staff/sales/{actualSaleId}?payment_error={Uri.EscapeDataString(result.Message)}");
    }


    private bool CanManagePayments() => User.IsInRole("Admin") || User.IsInRole("Staff");


    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
