using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/service-records")]
public class ServiceRecordsController(IServiceRecordService serviceRecordService) : ControllerBase
{
}
