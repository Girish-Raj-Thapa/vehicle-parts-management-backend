using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class VehicleRepository(AppDbContext context)
    : RepositoryBase<Vehicle>(context), IVehicleRepository
{
    /// <summary>
    /// Retrieves one vehicle by identifier.
    /// </summary>
    public Task<Vehicle?> GetByIdAsync(int vehicleId) =>
        Context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);


    /// <summary>
    /// Checks whether a vehicle belongs to a specific application user.
    /// </summary>
    public Task<bool> IsOwnedByUserAsync(int vehicleId, string applicationUserId) =>
        Context.Vehicles
            .AsNoTracking()
            .AnyAsync(v => v.VehicleId == vehicleId && v.ApplicationUserId == applicationUserId);
}
