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
}
