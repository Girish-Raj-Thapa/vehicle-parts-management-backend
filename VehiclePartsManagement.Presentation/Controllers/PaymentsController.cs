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
    public async Task<IActionResult> KhaltiCallback([FromQuery] string pidx, [FromQuery] int? saleId)
    {
        if (string.IsNullOrWhiteSpace(pidx))
        {
            return BadRequest(ApiResponse.FailureResponse("pidx is required."));
        }

        var result = await paymentService.VerifyKhaltiPaymentAsync(
            new KhaltiVerifyPaymentDto { Pidx = pidx, SaleId = saleId },
            userId: null,
            canManagePayments: true);

        return result.Success ? Ok(result) : BadRequest(result);
    }


    private bool CanManagePayments() => User.IsInRole("Admin") || User.IsInRole("Staff");


    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
