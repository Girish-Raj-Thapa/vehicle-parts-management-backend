using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
}
