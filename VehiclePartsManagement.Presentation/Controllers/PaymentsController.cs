using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
}
