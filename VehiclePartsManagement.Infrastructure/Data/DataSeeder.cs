using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Sales.AnyAsync()) return;

        // Seed Brand
        var brand = new VehicleBrand { BrandName = "Toyota" };
        context.VehicleBrands.Add(brand);
        await context.SaveChangesAsync();

        // Seed Model
        var model = new VehicleModel { ModelName = "Corolla", VehicleBrandId = brand.VehicleBrandId };
        context.VehicleModels.Add(model);
        await context.SaveChangesAsync();

        // Get Users
        var customer = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer@gmail.com");
        var staff = await context.Users.FirstOrDefaultAsync(u => u.Email == "staff@gmail.com");

        if (customer == null || staff == null) return;

        // Seed Vehicle
        var vehicle = new Vehicle
        {
            ApplicationUserId = customer.Id,
            VehicleModelId = model.VehicleModelId,
            VehicleNumber = "BA 1 PA 1234",
            FuelType = FuelType.Petrol,
            Color = "White"
        };
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        // Seed Part Category
        var category = new PartCategory { CategoryName = "Engine" };
        context.PartCategories.Add(category);
        await context.SaveChangesAsync();

        // Seed Part
        var part = new Part
        {
            PartName = "Oil Filter",
            Sku = "OF-001",
            PartCategoryId = category.PartCategoryId,
            SellingPrice = 500,
            CurrentStock = 100
        };
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        // Seed Sale
        var sale = new Sale
        {
            CustomerApplicationUserId = customer.Id,
            ProcessedByApplicationUserId = staff.Id,
            VehicleId = vehicle.VehicleId,
            SaleInvoiceNo = "INV-001",
            SaleDate = DateTime.UtcNow,
            SaleStatus = SaleStatus.Completed,
            PaymentStatus = PaymentStatus.Paid,
            SubTotal = 500,
            TotalAmount = 500,
            BalanceDue = 0
        };
        context.Sales.Add(sale);
        await context.SaveChangesAsync();

        // Seed Sale Item
        var saleItem = new SaleItem
        {
            SaleId = sale.SaleId,
            PartId = part.PartId,
            QuantitySold = 1,
            UnitPriceAtSale = 500,
            LineTotal = 500
        };
        context.SaleItems.Add(saleItem);
        // Seed Low Stock Part
        var lowStockPart = new Part
        {
            PartName = "Brake Pads",
            Sku = "BP-999",
            PartCategoryId = category.PartCategoryId,
            SellingPrice = 1200,
            CurrentStock = 2,
            LowStockThreshold = 10,
            IsActive = true
        };
        context.Parts.Add(lowStockPart);

        // Seed Overdue Sale
        var overdueSale = new Sale
        {
            CustomerApplicationUserId = customer.Id,
            ProcessedByApplicationUserId = staff.Id,
            VehicleId = vehicle.VehicleId,
            SaleInvoiceNo = "INV-OVERDUE",
            SaleDate = DateTime.UtcNow.AddMonths(-2),
            DueDate = DateTime.UtcNow.AddMonths(-1),
            SaleStatus = SaleStatus.Completed,
            PaymentStatus = PaymentStatus.PartiallyPaid,
            SubTotal = 1000,
            TotalAmount = 1000,
            BalanceDue = 400
        };
        context.Sales.Add(overdueSale);

        await context.SaveChangesAsync();
    }
}
