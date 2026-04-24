using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
}
