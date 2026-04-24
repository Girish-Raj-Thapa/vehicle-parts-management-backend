using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/service-appointments")]
public class ServiceAppointmentsController(IServiceAppointmentService serviceAppointmentService) : ControllerBase
{
}
