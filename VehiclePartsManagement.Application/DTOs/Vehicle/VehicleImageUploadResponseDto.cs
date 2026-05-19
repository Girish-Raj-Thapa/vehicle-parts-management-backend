namespace VehiclePartsManagement.Application.DTOs.Vehicle;

public class VehicleImageUploadResponseDto
{
    public int VehicleId { get; set; }

    public List<string> ImagePaths { get; set; } = [];
}
