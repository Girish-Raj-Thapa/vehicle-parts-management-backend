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
        var brand = await context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Toyota");
        if (brand == null)
        {
            brand = new VehicleBrand { BrandName = "Toyota" };
            context.VehicleBrands.Add(brand);
            await context.SaveChangesAsync();
        }

        // Seed Model
        var model = await context.VehicleModels.FirstOrDefaultAsync(m => m.ModelName == "Corolla" && m.VehicleBrandId == brand.VehicleBrandId);
        if (model == null)
        {
            model = new VehicleModel { ModelName = "Corolla", VehicleBrandId = brand.VehicleBrandId };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();
        }

        // Get Users
        var customer = await context.Users.FirstOrDefaultAsync(u => u.Email == "awantikapokharel46@gmail.com");
        var staff = await context.Users.FirstOrDefaultAsync(u => u.Email == "staff@gmail.com");

        if (customer == null || staff == null) return;

        // Seed Vehicle
        var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.VehicleNumber == "BA 1 PA 1234");
        if (vehicle == null)
        {
            vehicle = new Vehicle
            {
                ApplicationUserId = customer.Id,
                VehicleModelId = model.VehicleModelId,
                VehicleNumber = "BA 1 PA 1234",
                FuelType = FuelType.Petrol,
                Color = "White"
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
        }

        // Seed Part Category
        var category = await context.PartCategories.FirstOrDefaultAsync(c => c.CategoryName == "Engine");
        if (category == null)
        {
            category = new PartCategory { CategoryName = "Engine" };
            context.PartCategories.Add(category);
            await context.SaveChangesAsync();
        }

        // Seed Part
        var part = await context.Parts.FirstOrDefaultAsync(p => p.Sku == "OF-001");
        if (part == null)
        {
            part = new Part
            {
                PartName = "Oil Filter",
                Sku = "OF-001",
                PartCategoryId = category.PartCategoryId,
                SellingPrice = 500,
                CurrentStock = 100
            };
            context.Parts.Add(part);
            await context.SaveChangesAsync();
        }

        // Seed Sale
        var sale = await context.Sales.FirstOrDefaultAsync(s => s.SaleInvoiceNo == "INV-001");
        if (sale == null)
        {
            sale = new Sale
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
        }

        // Seed Low Stock Part
        if (!await context.Parts.AnyAsync(p => p.Sku == "BP-999"))
        {
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
        }

        // Seed Overdue Sale
        if (!await context.Sales.AnyAsync(s => s.SaleInvoiceNo == "INV-OVERDUE"))
        {
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
        }

        await context.SaveChangesAsync();
    }
}
