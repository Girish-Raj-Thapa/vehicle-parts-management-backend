using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class CartRepository(AppDbContext context) : RepositoryBase<Cart>(context), ICartRepository
{
    public async Task<Cart> GetOrCreateByCustomerWithItemsAsync(string customerUserId, bool trackChanges)
    {
        var query = Context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Part)
            .Where(c => c.CustomerApplicationUserId == customerUserId);

        var cart = trackChanges
            ? await query.FirstOrDefaultAsync()
            : await query.AsNoTracking().FirstOrDefaultAsync();

        if (cart is not null)
        {
            return cart;
        }

        var created = new Cart
        {
            CustomerApplicationUserId = customerUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Context.Carts.Add(created);
        await Context.SaveChangesAsync();

        var createdQuery = Context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Part)
            .Where(c => c.CartId == created.CartId);

        return trackChanges
            ? await createdQuery.FirstAsync()
            : await createdQuery.AsNoTracking().FirstAsync();
    }
}
