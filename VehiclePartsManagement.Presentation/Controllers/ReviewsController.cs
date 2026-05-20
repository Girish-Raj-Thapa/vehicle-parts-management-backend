using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Review;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    /// <summary>
    /// Submits a service review for the authenticated customer.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await reviewService.CreateAsync(dto, userId);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }


    /// <summary>
    /// Returns all reviews for administrative/staff visibility.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll()
    {
        var result = await reviewService.GetAllAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// Returns featured reviews for role-based consumers.
    /// </summary>
    [HttpGet("featured")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetFeatured([FromQuery] int limit = 10)
    {
        var result = await reviewService.GetFeaturedAsync(limit);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// Returns one review detail; customer access is constrained to own reviews.
    /// </summary>
    [HttpGet("{reviewId:int}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetById(int reviewId)
    {
        var scopedUserId = User.IsInRole("Customer") ? GetCurrentUserId() : null;
        if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(scopedUserId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await reviewService.GetByIdAsync(reviewId, scopedUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }


    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
