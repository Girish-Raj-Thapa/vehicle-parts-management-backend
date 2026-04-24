using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class ReviewRepository(AppDbContext context)
    : RepositoryBase<Review>(context), IReviewRepository
{
}
