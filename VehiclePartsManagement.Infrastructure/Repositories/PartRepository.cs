using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class PartRepository(AppDbContext context)
    : RepositoryBase<Part>(context), IPartRepository
{
    public async Task<IEnumerable<Part>> GetLowStockPartsAsync()
    {
        return await Context.Parts
            .Include(p => p.Category)
            .Where(p => p.CurrentStock <= p.LowStockThreshold)
            .ToListAsync();
    }
}
