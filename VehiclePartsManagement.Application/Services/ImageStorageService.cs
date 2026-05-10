using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using VehiclePartsManagement.Application.Interfaces;
using VehiclePartsManagement.Application.Config;

namespace VehiclePartsManagement.Application.Services;

/// <summary>
/// Handles image validation, optimization, and file-system persistence.
/// </summary>
public class ImageStorageService(
    IWebHostEnvironment environment,
    IOptions<ImageStorageOptions> imageStorageOptions) : IImageStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly ImageStorageOptions _options = imageStorageOptions.Value;

    /// <inheritdoc />
    public async Task<string> SaveImageAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSizeInBytes,
        string folder,
        CancellationToken cancellationToken = default)
    {
        ValidateImageInput(fileStream, originalFileName, contentType, fileSizeInBytes);

        var normalizedFolder = NormalizeFolderSegment(folder);
        var requestPath = NormalizeRequestPath(_options.RequestPath);
        var fileName = $"{Guid.NewGuid():N}.webp";

        var absoluteRoot = GetStorageRootPath();
        var absoluteDirectory = Path.Combine(absoluteRoot, normalizedFolder);
        Directory.CreateDirectory(absoluteDirectory);

        var absoluteFilePath = Path.Combine(absoluteDirectory, fileName);

        await using var readBuffer = new MemoryStream();
        await fileStream.CopyToAsync(readBuffer, cancellationToken);
        readBuffer.Position = 0;

        using var image = await Image.LoadAsync(readBuffer, cancellationToken);

        image.Mutate(ctx =>
        {
            ctx.AutoOrient();

            if (image.Width > _options.MaxImageDimension || image.Height > _options.MaxImageDimension)
            {
                ctx.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(_options.MaxImageDimension, _options.MaxImageDimension)
                });
            }
        });

        await image.SaveAsync(
            absoluteFilePath,
            new WebpEncoder
            {
                Quality = _options.WebpQuality
            },
            cancellationToken);

        return $"{requestPath}/{normalizedFolder}/{fileName}".Replace("//", "/");
    }


    /// <inheritdoc />
    public async Task<string> ReplaceImageAsync(
        string? existingRelativePath,
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSizeInBytes,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var newRelativePath = await SaveImageAsync(
            fileStream,
            originalFileName,
            contentType,
            fileSizeInBytes,
            folder,
            cancellationToken);

        try
        {
            await DeleteImageAsync(existingRelativePath, cancellationToken);
        }
        catch
        {
            // Keep operation successful even if old file cleanup fails.
        }

        return newRelativePath;
    }


    /// <inheritdoc />
    public Task DeleteImageAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var requestPath = NormalizeRequestPath(_options.RequestPath);
        if (!relativePath.StartsWith(requestPath, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var relativeFilePortion = relativePath[requestPath.Length..].TrimStart('/');
        if (string.IsNullOrWhiteSpace(relativeFilePortion))
        {
            return Task.CompletedTask;
        }

        var absoluteRoot = GetStorageRootPath();
        var combinedPath = Path.Combine(
            absoluteRoot,
            relativeFilePortion.Replace('/', Path.DirectorySeparatorChar));
        var fullPath = Path.GetFullPath(combinedPath);

        if (!fullPath.StartsWith(Path.GetFullPath(absoluteRoot), StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }


    private void ValidateImageInput(
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSizeInBytes)
    {
        if (!fileStream.CanRead)
        {
            throw new ArgumentException("File stream is not readable.", nameof(fileStream));
        }

        var maxBytes = _options.MaxUploadSizeMb * 1024L * 1024L;
        if (fileSizeInBytes <= 0 || fileSizeInBytes > maxBytes)
        {
            throw new ArgumentException($"Image size must be between 1 byte and {_options.MaxUploadSizeMb} MB.");
        }

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Unsupported image extension.");
        }

        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType))
        {
            throw new ArgumentException("Unsupported image content type.");
        }
    }


    private string GetStorageRootPath() =>
        Path.Combine(environment.ContentRootPath, _options.RelativeStoragePath);


    private static string NormalizeRequestPath(string requestPath)
    {
        var normalized = requestPath.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized.TrimEnd('/');
    }


    private static string NormalizeFolderSegment(string folder)
    {
        var safeParts = folder
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => new string(part.Where(ch =>
                char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray()))
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        return safeParts.Length == 0 ? "general" : string.Join('/', safeParts);
    }
}
