using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Vehicle;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    /// <summary>
    /// Registers a new vehicle for the authenticated user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] VehicleRegistrationDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await vehicleService.RegisterAsync(dto, userId);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }


    /// <summary>
    /// Returns a paged list of vehicles with optional mine/search filters.
    /// Admin/Staff see all vehicles; Customer is forced to own vehicles only.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetPaged([FromQuery] VehicleQueryDto query)
    {
        var isCustomer = User.IsInRole("Customer");
        var userId = isCustomer || query.Mine ? GetCurrentUserId() : null;
        if ((isCustomer || query.Mine) && string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        // Force customer scope for Customer role
        if (isCustomer)
        {
            query.Mine = true;
        }

        var result = await vehicleService.GetPagedAsync(query, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// Returns full vehicle detail. Customer access is scoped to own vehicles.
    /// </summary>
    [HttpGet("{vehicleId:int}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetById(int vehicleId)
    {
        var scopedUserId = User.IsInRole("Customer") ? GetCurrentUserId() : null;
        if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(scopedUserId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await vehicleService.GetByIdAsync(vehicleId, scopedUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }


    /// <summary>
    /// Uploads one or more images for a vehicle.
    /// Customer access is scoped to own vehicles.
    /// </summary>
    [HttpPost("{vehicleId:int}/images")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImages(
        int vehicleId,
        [FromForm] List<IFormFile>? images,
        [FromForm] bool makeFirstImagePrimary = false,
        CancellationToken cancellationToken = default)
    {
        var scopedUserId = User.IsInRole("Customer") ? GetCurrentUserId() : null;
        if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(scopedUserId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var openedStreams = new List<Stream>();
        try
        {
            var files = (images ?? []).Select(image =>
            {
                var stream = image.OpenReadStream();
                openedStreams.Add(stream);

                return new VehicleImageUploadFileDto
                {
                    FileStream = stream,
                    FileName = image.FileName,
                    ContentType = image.ContentType,
                    Length = image.Length
                };
            }).ToList();

            var result = await vehicleService.AddImagesAsync(
                vehicleId,
                scopedUserId,
                files,
                makeFirstImagePrimary,
                cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        finally
        {
            foreach (var stream in openedStreams)
            {
                await stream.DisposeAsync();
            }
        }
    }


    /// <summary>
    /// Returns paged service history for a vehicle.
    /// Customer access is scoped to own vehicles.
    /// </summary>
    [HttpGet("{vehicleId:int}/service-history")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetServiceHistory(
        int vehicleId,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 10)
    {
        var scopedUserId = User.IsInRole("Customer") ? GetCurrentUserId() : null;
        if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(scopedUserId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await vehicleService.GetServiceHistoryAsync(vehicleId, scopedUserId, page, pageSize);
        return result.Success ? Ok(result) : NotFound(result);
    }


    /// <summary>
    /// Returns paged sale/purchase history for a vehicle.
    /// Customer access is scoped to own vehicles.
    /// </summary>
    [HttpGet("{vehicleId:int}/sale-history")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetSaleHistory(
        int vehicleId,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 10)
    {
        var scopedUserId = User.IsInRole("Customer") ? GetCurrentUserId() : null;
        if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(scopedUserId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await vehicleService.GetSaleHistoryAsync(vehicleId, scopedUserId, page, pageSize);
        return result.Success ? Ok(result) : NotFound(result);
    }


    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
