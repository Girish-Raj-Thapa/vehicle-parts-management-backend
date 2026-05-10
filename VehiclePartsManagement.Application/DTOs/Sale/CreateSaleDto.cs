using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Request payload for creating a sale invoice and selling parts.
/// </summary>
public class CreateSaleDto
{
    [Required]
    public string CustomerUserId { get; set; } = string.Empty;

    [Required]
    public int VehicleId { get; set; }

    [MinLength(1)]
    public List<CreateSaleItemDto> Items { get; set; } = [];

    public CreateSalePaymentDto? Payment { get; set; }

    public bool IsCreditSale { get; set; }

    public DateOnly? DueDate { get; set; }
}
