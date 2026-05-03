using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Vehicle;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] VehicleRegistrationDto dto)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await vehicleService.RegisterAsync(dto, userId);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }
}
