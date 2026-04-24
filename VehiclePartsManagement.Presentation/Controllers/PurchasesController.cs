using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/purchases")]
public class PurchasesController(IPurchaseService purchaseService) : ControllerBase
{
}
