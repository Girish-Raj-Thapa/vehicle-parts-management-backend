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
    public Task<Part?> GetByIdAsync(int partId) =>
        Context.Parts.FirstOrDefaultAsync(p => p.PartId == partId);

    public Task<List<Part>> GetByIdsAsync(List<int> partIds) =>
        Context.Parts
            .Where(p => partIds.Contains(p.PartId))
            .ToListAsync();

    public Task<Part?> GetByIdWithDetailsAsync(int partId) =>
        Context.Parts
            .Include(p => p.Category)
            .Include(p => p.PartImages)
            .Include(p => p.PartCompatibilities)
                .ThenInclude(pc => pc.Model)
                    .ThenInclude(m => m.Brand)
            .FirstOrDefaultAsync(p => p.PartId == partId);

    public IQueryable<Part> GetPartsQuery(bool trackChanges = false)
    {
        var query = Context.Parts
            .Include(p => p.Category)
            .Include(p => p.PartImages)
            .Include(p => p.PartCompatibilities)
                .ThenInclude(pc => pc.Model)
                    .ThenInclude(m => m.Brand)
            .AsQueryable();

        return trackChanges ? query : query.AsNoTracking();
    }

    public IQueryable<Part> SearchParts(IQueryable<Part> query, string searchTerm)
    {
        var pattern = $"%{searchTerm}%";
        return query.Where(p => 
            EF.Functions.ILike(p.PartName, pattern) || 
            EF.Functions.ILike(p.Sku, pattern) || 
            EF.Functions.ILike(p.PartDescription, pattern));
    }

    public Task<List<Part>> GetFeaturedPartsAsync(int limit = 5) =>
        Context.Parts
            .Include(p => p.Category)
            .Include(p => p.PartImages)
            .Include(p => p.PartCompatibilities)
                .ThenInclude(pc => pc.Model)
                    .ThenInclude(m => m.Brand)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.PartId) // Just using ID desc as a proxy for featured
            .Take(limit)
            .ToListAsync();

    public Task<List<Part>> GetRelatedPartsAsync(int partId, int categoryId, int limit = 5) =>
        Context.Parts
            .Include(p => p.Category)
            .Include(p => p.PartImages)
            .Include(p => p.PartCompatibilities)
                .ThenInclude(pc => pc.Model)
                    .ThenInclude(m => m.Brand)
            .Where(p => p.IsActive && p.PartCategoryId == categoryId && p.PartId != partId)
            .Take(limit)
            .ToListAsync();
}
