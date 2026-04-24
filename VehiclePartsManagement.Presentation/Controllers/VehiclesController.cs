using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
}
