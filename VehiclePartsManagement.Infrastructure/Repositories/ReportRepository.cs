using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class ReportRepository(AppDbContext context) : IReportRepository
{
    protected AppDbContext Context { get; } = context;

    public async Task<List<Sale>> GetSalesWithDetailsAsync(DateTime fromUtc, DateTime toUtc, bool includeCancelled)
    {
        var query = Context.Sales
            .AsNoTracking()
            .Include(s => s.CustomerUser)
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Part)
            .Include(s => s.Payments)
            .Where(s => s.SaleDate >= fromUtc && s.SaleDate <= toUtc);

        if (!includeCancelled)
        {
            query = query.Where(s => s.SaleStatus != SaleStatus.Cancelled);
        }
        
        return await query.ToListAsync();
    }


    public async Task<List<Sale>> GetSalesWithBalancesAsync(DateTime fromUtc, DateTime toUtc, bool includeCancelled)
    {
        var query = Context.Sales
            .AsNoTracking()
            .Include(s => s.CustomerUser)
            .Where(s => s.SaleDate >= fromUtc && s.SaleDate <= toUtc);
        
        if (!includeCancelled)
        {
            query = query.Where(s => s.SaleStatus != SaleStatus.Cancelled);
        }

        return await query.ToListAsync();
    }


    public async Task<List<Purchase>> GetPurchasesWithDetailsAsync(DateTime fromUtc, DateTime toUtc,
        bool includeCancelled)
    {
        var query = Context.Purchases
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Part)
            .Where(p => p.PurchaseDate >= fromUtc && p.PurchaseDate <= toUtc);
        if (!includeCancelled)
        {
            query = query.Where(p => p.PurchaseStatus != PurchaseStatus.Cancelled);
        }

        return await query.ToListAsync();
    }
    
    
    public async Task<List<Payment>> GetPaymentsWithSaleAsync(DateTime fromUtc, DateTime toUtc)
    {
        return await Context.Payments
            .AsNoTracking()
            .Include(p => p.Sale)
            .ThenInclude(s => s.CustomerUser)
            .Where(p => p.PaymentDate >= fromUtc && p.PaymentDate <= toUtc)
            .ToListAsync();
    }

    
    public async Task<List<ServiceRecord>> GetServiceRecordsAsync(DateTime fromUtc, DateTime toUtc)
    {
        return await Context.ServiceRecords
            .AsNoTracking()
            .Where(sr => sr.ServiceDate >= fromUtc && sr.ServiceDate <= toUtc)
            .ToListAsync();
    }

    
    public async Task<List<Sale>> GetOutstandingSalesAsync()
    {
        return await Context.Sales
            .AsNoTracking()
            .Include(s => s.CustomerUser)
            .Where(s => s.BalanceDue > 0)
            .ToListAsync();
    }

    
    public async Task<List<Part>> GetLowStockPartsAsync()
    {
        return await Context.Parts
            .AsNoTracking()
            .Where(p => p.IsActive && p.CurrentStock <= p.LowStockThreshold)
            .ToListAsync();
    }
}
