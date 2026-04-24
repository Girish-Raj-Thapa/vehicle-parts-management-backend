namespace VehiclePartsManagement.Application.Interfaces;

/// <summary>
/// Provides centralized image validation, optimization, and file storage operations.
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Saves an image after validation and optimization, then returns its public relative path.
    /// </summary>
    Task<string> SaveImageAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSizeInBytes,
        string folder,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Replaces an existing image path with a new optimized image and returns the new path.
    /// </summary>
    Task<string> ReplaceImageAsync(
        string? existingRelativePath,
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSizeInBytes,
        string folder,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Deletes an image if it exists.
    /// </summary>
    Task DeleteImageAsync(string? relativePath, CancellationToken cancellationToken = default);
}
