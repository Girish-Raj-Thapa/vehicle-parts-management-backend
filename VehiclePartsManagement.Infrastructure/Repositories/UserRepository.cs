using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class UserRepository(AppDbContext context)
    : RepositoryBase<ApplicationUser>(context), IUserRepository
{
    public Task<ApplicationUser?> GetCustomerWithVehiclesAsync(string customerId) =>
        Context.Users
            .Include(user => user.Vehicles)
                .ThenInclude(vehicle => vehicle.Model)
                    .ThenInclude(model => model.Brand)
            .FirstOrDefaultAsync(user => user.Id == customerId);
}
