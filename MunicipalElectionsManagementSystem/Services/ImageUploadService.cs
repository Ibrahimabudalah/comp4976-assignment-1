
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MunicipalElectionsManagementSystem.Services;

public class ImageUploadService
{
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public ImageUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveImageAsync(
        IFormFile? file,
        string folder)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        if (file.Length > 2 * 1024 * 1024)
        {
            throw new ArgumentException(
                "Image must be smaller than 2 MB.");
        }

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Only JPG, PNG, and WebP images are allowed.");
        }

        var root = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var uploadDirectory = Path.Combine(
            root, "uploads", folder);

        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using var stream = new FileStream(
            filePath, FileMode.CreateNew);

        await file.CopyToAsync(stream);

        return fileName;
    }

    public void DeleteImage(string? fileName, string folder)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var root = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var safeFileName = Path.GetFileName(fileName);

        var filePath = Path.Combine(
            root, "uploads", folder, safeFileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}