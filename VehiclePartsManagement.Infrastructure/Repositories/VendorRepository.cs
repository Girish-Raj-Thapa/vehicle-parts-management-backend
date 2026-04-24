using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class VendorRepository(AppDbContext context)
    : RepositoryBase<Vendor>(context), IVendorRepository
{
}
