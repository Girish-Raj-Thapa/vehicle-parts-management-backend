using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IPartRepository : IRepositoryBase<Part>
{
    Task<Part?> GetByIdAsync(int partId);
    Task<Part?> GetByIdWithDetailsAsync(int partId);
    IQueryable<Part> GetPartsQuery(bool trackChanges = false);
    IQueryable<Part> SearchParts(IQueryable<Part> query, string searchTerm);
    Task<List<Part>> GetFeaturedPartsAsync(int limit = 5);
    Task<List<Part>> GetRelatedPartsAsync(int partId, int categoryId, int limit = 5);
}
