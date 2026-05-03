using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class PartRepository(AppDbContext context)
    : RepositoryBase<Part>(context), IPartRepository
{
    public Task<Part?> GetByIdAsync(int partId) =>
        Context.Parts.FirstOrDefaultAsync(p => p.PartId == partId);

    public Task<Part?> GetByIdWithCategoryAsync(int partId) =>
        Context.Parts
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.PartId == partId);

    public Task<List<Part>> GetAllWithCategoryAsync() =>
        Context.Parts
            .Include(p => p.Category)
            .OrderBy(p => p.PartName)
            .ToListAsync();
}
