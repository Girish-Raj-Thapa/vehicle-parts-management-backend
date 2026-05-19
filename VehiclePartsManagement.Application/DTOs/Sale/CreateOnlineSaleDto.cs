using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Request payload for customer self-checkout part purchase.
/// </summary>
public class CreateOnlineSaleDto
{
    [MinLength(1)]
    public List<CreateSaleItemDto> Items { get; set; } = [];
}
