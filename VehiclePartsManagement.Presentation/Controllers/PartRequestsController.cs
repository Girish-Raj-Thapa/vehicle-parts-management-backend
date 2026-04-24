using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/part-requests")]
public class PartRequestsController(IPartRequestService partRequestService) : ControllerBase
{
}
