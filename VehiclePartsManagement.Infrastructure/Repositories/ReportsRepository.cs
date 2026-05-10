using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.DTOs.Report;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class ReportsRepository(AppDbContext context) : IReportsRepository
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
    
    public async Task<DateTime?> GetDashboardStartDateUtcAsync()
    {
        var saleMin = await Context.Sales
            .AsNoTracking()
            .Select(s => (DateTime?)s.SaleDate)
            .MinAsync();

        var purchaseMin = await Context.Purchases
            .AsNoTracking()
            .Select(p => (DateTime?)p.PurchaseDate)
            .MinAsync();

        var paymentMin = await Context.Payments
            .AsNoTracking()
            .Select(p => (DateTime?)p.PaymentDate)
            .MinAsync();

        var serviceMin = await Context.ServiceRecords
            .AsNoTracking()
            .Select(sr => (DateTime?)sr.ServiceDate)
            .MinAsync();

        return new[] { saleMin, purchaseMin, paymentMin, serviceMin }
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .DefaultIfEmpty()
            .Min();
    }

    public async Task<IEnumerable<RegularCustomerReportDto>> GetRegularCustomersAsync()
    {
        return await Context.Sales
            .GroupBy(s => s.CustomerApplicationUserId)
            .Where(g => g.Count() > 1)
            .Select(g => new RegularCustomerReportDto
            {
                CustomerId = g.Key,
                FullName = g.First().CustomerUser.FullName,
                Email = g.First().CustomerUser.Email ?? string.Empty,
                PhoneNumber = g.First().CustomerUser.PhoneNumber ?? string.Empty,
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(s => s.TotalAmount),
                LastPurchaseDate = g.Max(s => s.SaleDate)
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<TopSpenderReportDto>> GetTopSpendersAsync()
    {
        return await Context.Sales
            .GroupBy(s => s.CustomerApplicationUserId)
            .Select(g => new TopSpenderReportDto
            {
                CustomerId = g.Key,
                FullName = g.First().CustomerUser.FullName,
                Email = g.First().CustomerUser.Email ?? string.Empty,
                PhoneNumber = g.First().CustomerUser.PhoneNumber ?? string.Empty,
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(s => s.TotalAmount)
            })
            .OrderByDescending(dto => dto.TotalSpent)
            .ToListAsync();
    }

    public async Task<IEnumerable<PendingCreditReportDto>> GetPendingCreditsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await Context.Sales
            .Where(s => s.BalanceDue > 0)
            .Select(s => new PendingCreditReportDto
            {
                CustomerId = s.CustomerApplicationUserId,
                FullName = s.CustomerUser.FullName,
                Email = s.CustomerUser.Email ?? string.Empty,
                PhoneNumber = s.CustomerUser.PhoneNumber ?? string.Empty,
                SaleId = s.SaleId,
                InvoiceNo = s.SaleInvoiceNo,
                TotalAmount = s.TotalAmount,
                PaidAmount = s.TotalAmount - s.BalanceDue,
                BalanceDue = s.BalanceDue,
                DueDate = s.DueDate,
                IsOverdue = s.DueDate < today
            })
            .ToListAsync();
    }
}
