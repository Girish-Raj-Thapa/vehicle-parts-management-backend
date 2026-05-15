using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Vehicle;
using VehiclePartsManagement.Application.Interfaces;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

public class VehicleService(
    IVehicleRepository vehicleRepository,
    IRepositoryBase<VehicleModel> vehicleModelRepository,
    IRepositoryBase<VehicleBrand> vehicleBrandRepository) : IVehicleService
{
    // ────────────────────────────────────────────────────────────────
    // Register
    // ────────────────────────────────────────────────────────────────

    public async Task<ApiResponse<VehicleRegistrationResponseDto>> RegisterAsync(
        VehicleRegistrationDto dto,
        string userId)
    {
        var vehicleExists = vehicleRepository
            .FindByCondition(v => v.VehicleNumber == dto.VehicleNumber)
            .Any();

        if (vehicleExists)
        {
            return ApiResponse<VehicleRegistrationResponseDto>.FailureResponse("Vehicle number already exists.");
        }

        var (modelId, errorMessage) = await ResolveVehicleModelIdAsync(dto);
        if (errorMessage is not null)
        {
            return ApiResponse<VehicleRegistrationResponseDto>.FailureResponse(errorMessage);
        }

        var vehicle = new Vehicle
        {
            ApplicationUserId = userId,
            VehicleModelId = modelId,
            VehicleNumber = dto.VehicleNumber,
            ManufactureYear = dto.ManufactureYear,
            EngineNumber = dto.EngineNumber,
            ChassisNumber = dto.ChassisNumber,
            FuelType = dto.FuelType,
            Color = dto.Color,
            CurrentMileage = dto.CurrentMileage,
            VehicleRegisteredAt = DateTime.UtcNow
        };

        vehicleRepository.Create(vehicle);
        await vehicleRepository.SaveChangesAsync();

        return ApiResponse<VehicleRegistrationResponseDto>.SuccessResponse(
            new VehicleRegistrationResponseDto { VehicleId = vehicle.VehicleId },
            "Vehicle registered successfully.");
    }


    // ────────────────────────────────────────────────────────────────
    // Paged Vehicle List
    // ────────────────────────────────────────────────────────────────

    public async Task<PagedResponse<List<VehicleListItemDto>>> GetPagedAsync(
        VehicleQueryDto query,
        string? applicationUserId)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

        var scopedUserId = query.Mine ? applicationUserId : null;
        var (items, totalItems) = await vehicleRepository.GetPagedAsync(page, pageSize, scopedUserId, query.Search);

        var data = items.Select(MapListItem).ToList();

        return PagedResponse<List<VehicleListItemDto>>.SuccessPagedResponse(
            data,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Vehicles retrieved successfully.");
    }


    // ────────────────────────────────────────────────────────────────
    // Vehicle Detail
    // ────────────────────────────────────────────────────────────────

    public async Task<ApiResponse<VehicleDetailDto>> GetByIdAsync(
        int vehicleId,
        string? applicationUserId)
    {
        var vehicle = string.IsNullOrWhiteSpace(applicationUserId)
            ? await vehicleRepository.GetByIdWithDetailsAsync(vehicleId)
            : await vehicleRepository.GetByIdForUserAsync(vehicleId, applicationUserId);

        if (vehicle is null)
        {
            return ApiResponse<VehicleDetailDto>.FailureResponse("Vehicle not found.");
        }

        return ApiResponse<VehicleDetailDto>.SuccessResponse(
            MapDetail(vehicle),
            "Vehicle retrieved successfully.");
    }


    // ────────────────────────────────────────────────────────────────
    // Service History
    // ────────────────────────────────────────────────────────────────

    public async Task<PagedResponse<List<VehicleServiceHistoryDto>>> GetServiceHistoryAsync(
        int vehicleId,
        string? applicationUserId,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        // Verify ownership when scoped to a user
        if (!string.IsNullOrWhiteSpace(applicationUserId))
        {
            var isOwner = await vehicleRepository.IsVehicleOwnedByUserAsync(vehicleId, applicationUserId);
            if (!isOwner)
            {
                return PagedFailure<VehicleServiceHistoryDto>(
                    "Vehicle not found or access denied.", "vehicleId");
            }
        }

        var (items, totalItems) = await vehicleRepository.GetServiceHistoryAsync(vehicleId, page, pageSize);

        var data = items.Select(sr => new VehicleServiceHistoryDto
        {
            ServiceRecordId = sr.ServiceRecordId,
            AppointmentId = sr.ServiceAppointmentId,
            ServiceDate = sr.ServiceDate,
            ServiceType = sr.ServiceType,
            Diagnosis = sr.Diagnosis,
            WorkDone = sr.WorkDone,
            MileageAtService = sr.MileageAtService,
            LaborCharge = sr.LaborCharge,
            NextServiceDueDate = sr.NextServiceDueDate,
            ReviewRating = sr.Review?.Rating,
            ReviewComment = sr.Review?.ReviewComment
        }).ToList();

        return PagedResponse<List<VehicleServiceHistoryDto>>.SuccessPagedResponse(
            data,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Service history retrieved successfully.");
    }


    // ────────────────────────────────────────────────────────────────
    // Sale History
    // ────────────────────────────────────────────────────────────────

    public async Task<PagedResponse<List<VehicleSaleHistoryDto>>> GetSaleHistoryAsync(
        int vehicleId,
        string? applicationUserId,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        // Verify ownership when scoped to a user
        if (!string.IsNullOrWhiteSpace(applicationUserId))
        {
            var isOwner = await vehicleRepository.IsVehicleOwnedByUserAsync(vehicleId, applicationUserId);
            if (!isOwner)
            {
                return PagedFailure<VehicleSaleHistoryDto>(
                    "Vehicle not found or access denied.", "vehicleId");
            }
        }

        var (items, totalItems) = await vehicleRepository.GetSaleHistoryAsync(vehicleId, page, pageSize);

        var data = items.Select(s => new VehicleSaleHistoryDto
        {
            SaleId = s.SaleId,
            SaleInvoiceNo = s.SaleInvoiceNo,
            SaleDate = s.SaleDate,
            SaleStatus = s.SaleStatus.ToString(),
            SubTotal = s.SubTotal,
            DiscountAmount = s.DiscountAmount,
            TotalAmount = s.TotalAmount,
            PaymentStatus = s.PaymentStatus.ToString(),
            ProcessedBy = s.ProcessedByUser.FullName,
            Items = s.SaleItems.Select(si => new VehicleSaleItemDto
            {
                PartId = si.PartId,
                PartName = si.Part.PartName,
                Sku = si.Part.Sku,
                QuantitySold = si.QuantitySold,
                UnitPrice = si.UnitPriceAtSale,
                LineTotal = si.LineTotal
            }).ToList()
        }).ToList();

        return PagedResponse<List<VehicleSaleHistoryDto>>.SuccessPagedResponse(
            data,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Sale history retrieved successfully.");
    }


    // ────────────────────────────────────────────────────────────────
    // Vehicles by Customer ID
    // ────────────────────────────────────────────────────────────────

    public async Task<ApiResponse<List<VehicleListItemDto>>> GetByCustomerIdAsync(string customerId)
    {
        var (items, _) = await vehicleRepository.GetPagedAsync(1, 1000, customerId, null);

        var data = items.Select(MapListItem).ToList();

        return ApiResponse<List<VehicleListItemDto>>.SuccessResponse(
            data,
            "Customer vehicles retrieved successfully.");
    }


    // ────────────────────────────────────────────────────────────────
    // Private Helpers
    // ────────────────────────────────────────────────────────────────

    private async Task<(int ModelId, string? ErrorMessage)> ResolveVehicleModelIdAsync(VehicleRegistrationDto dto)
    {
        if (dto.ModelId is > 0)
        {
            var existingModel = await vehicleModelRepository.GetByIdAsync(dto.ModelId.Value);
            return existingModel is null
                ? (0, "Vehicle model not found.")
                : (existingModel.VehicleModelId, null);
        }

        if (string.IsNullOrWhiteSpace(dto.BrandName) || string.IsNullOrWhiteSpace(dto.ModelName))
        {
            return (0, "Either modelId or both brandName and modelName are required.");
        }

        var brandName = dto.BrandName.Trim();
        var modelName = dto.ModelName.Trim();

        var brand = vehicleBrandRepository
            .FindByCondition(b => b.BrandName == brandName, trackChanges: true)
            .FirstOrDefault();

        if (brand is null)
        {
            brand = new VehicleBrand { BrandName = brandName };
            vehicleBrandRepository.Create(brand);
            await vehicleBrandRepository.SaveChangesAsync();
        }

        var model = vehicleModelRepository
            .FindByCondition(m => m.VehicleBrandId == brand.VehicleBrandId && m.ModelName == modelName, trackChanges: true)
            .FirstOrDefault();

        if (model is null)
        {
            model = new VehicleModel
            {
                VehicleBrandId = brand.VehicleBrandId,
                ModelName = modelName
            };
            vehicleModelRepository.Create(model);
            await vehicleModelRepository.SaveChangesAsync();
        }

        return (model.VehicleModelId, null);
    }


    private static VehicleListItemDto MapListItem(Vehicle vehicle) =>
        new()
        {
            VehicleId = vehicle.VehicleId,
            OwnerName = vehicle.User.FullName,
            BrandName = vehicle.Model.Brand.BrandName,
            ModelName = vehicle.Model.ModelName,
            VehicleName = $"{vehicle.Model.Brand.BrandName} {vehicle.Model.ModelName}",
            VehicleNumber = vehicle.VehicleNumber,
            ManufactureYear = vehicle.ManufactureYear,
            FuelType = vehicle.FuelType.ToString(),
            Color = vehicle.Color,
            CurrentMileage = vehicle.CurrentMileage,
            PrimaryImagePath = vehicle.VehicleImages
                .FirstOrDefault(i => i.IsPrimary)?.ImagePath
                ?? vehicle.VehicleImages.FirstOrDefault()?.ImagePath
        };


    private static VehicleDetailDto MapDetail(Vehicle vehicle) =>
        new()
        {
            VehicleId = vehicle.VehicleId,
            OwnerName = vehicle.User.FullName,
            OwnerEmail = vehicle.User.Email ?? string.Empty,
            OwnerPhone = vehicle.User.PhoneNumber ?? string.Empty,
            BrandName = vehicle.Model.Brand.BrandName,
            ModelName = vehicle.Model.ModelName,
            VehicleName = $"{vehicle.Model.Brand.BrandName} {vehicle.Model.ModelName}",
            VehicleNumber = vehicle.VehicleNumber,
            ManufactureYear = vehicle.ManufactureYear,
            EngineNumber = vehicle.EngineNumber,
            ChassisNumber = vehicle.ChassisNumber,
            FuelType = vehicle.FuelType.ToString(),
            Color = vehicle.Color,
            CurrentMileage = vehicle.CurrentMileage,
            VehicleRegisteredAt = vehicle.VehicleRegisteredAt,
            ImagePaths = vehicle.VehicleImages
                .OrderBy(i => i.DisplayOrder)
                .Select(i => i.ImagePath)
                .ToList(),
            TotalServiceRecords = vehicle.ServiceRecords.Count,
            TotalSales = vehicle.Sales.Count,
            TotalPartRequests = vehicle.PartRequests.Count
        };


    private static PagedResponse<List<T>> PagedFailure<T>(string message, string errorKey) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = new Dictionary<string, List<string>>
            {
                { errorKey, [message] }
            },
            Data = new List<T>(),
            Pagination = new PaginationMeta()
        };
}
