using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/vendors")]
public class VendorsController(IVendorService vendorService) : ControllerBase
{
}
