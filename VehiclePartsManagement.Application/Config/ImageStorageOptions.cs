using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.Config;

/// <summary>
/// Strongly typed image storage settings.
/// </summary>
public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    /// <summary>
    /// Public request path mapped to physical storage (for example: /resources/images).
    /// </summary>
    [Required]
    public string RequestPath { get; init; } = "/resources/images";

    /// <summary>
    /// Folder path relative to the content root where optimized images are stored.
    /// </summary>
    [Required]
    public string RelativeStoragePath { get; init; } = "Uploads/Images";

    /// <summary>
    /// Maximum allowed upload size in MB.
    /// </summary>
    [Range(1, 20)]
    public int MaxUploadSizeMb { get; init; } = 2;

    /// <summary>
    /// WebP encoding quality (1-100).
    /// </summary>
    [Range(1, 100)]
    public int WebpQuality { get; init; } = 75;

    /// <summary>
    /// Longest side resize bound used during optimization.
    /// </summary>
    [Range(256, 6000)]
    public int MaxImageDimension { get; init; } = 1600;
}
