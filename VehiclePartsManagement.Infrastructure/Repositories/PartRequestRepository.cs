using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class PartRequestRepository(AppDbContext context)
    : RepositoryBase<PartRequest>(context), IPartRequestRepository
{
}
