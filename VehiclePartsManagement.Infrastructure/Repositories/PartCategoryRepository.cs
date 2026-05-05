using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class PartCategoryRepository(AppDbContext context)
    : RepositoryBase<PartCategory>(context), IPartCategoryRepository
{
    public Task<PartCategory?> GetByIdAsync(int categoryId) =>
        Context.Set<PartCategory>().FirstOrDefaultAsync(c => c.PartCategoryId == categoryId);

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var normalized = name.Trim().ToLower();
        var query = Context.Set<PartCategory>().AsNoTracking()
            .Where(c => c.CategoryName.ToLower() == normalized);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.PartCategoryId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public Task<List<PartCategory>> GetAllOrderedAsync() =>
        Context.Set<PartCategory>()
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

    public Task<bool> HasLinkedPartsAsync(int categoryId) =>
        Context.Set<Part>().AsNoTracking().AnyAsync(p => p.PartCategoryId == categoryId);
}
