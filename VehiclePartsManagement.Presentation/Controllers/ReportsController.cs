using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IReportService reportService) : ControllerBase
{
}
