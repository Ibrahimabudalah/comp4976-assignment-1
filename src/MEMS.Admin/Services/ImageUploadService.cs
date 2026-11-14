using Microsoft.AspNetCore.Components.Forms;

namespace MEMS.Admin.Services;

public class ImageUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageUploadService> _logger;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };
    private const long MaxFileSize = 2 * 1024 * 1024; // 2MB

    public ImageUploadService(IWebHostEnvironment environment, ILogger<ImageUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(IBrowserFile file, string subfolder)
    {
        if (file.Size > MaxFileSize)
        {
            throw new ArgumentException("Image size exceeds the 2MB limit.");
        }

        var extension = Path.GetExtension(file.Name);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Invalid file format. Allowed formats are JPG, JPEG, PNG, and WebP.");
        }

        var targetFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", subfolder);
        Directory.CreateDirectory(targetFolder);

        var uniqueName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(targetFolder, uniqueName);

        await using var stream = file.OpenReadStream(MaxFileSize);
        await using var fs = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fs);

        _logger.LogInformation("Saved upload image {FileName} to {Folder}", uniqueName, subfolder);
        return uniqueName;
    }

    public void DeleteImage(string? fileName, string subfolder)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;

        try
        {
            var filePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", subfolder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted image {FileName} from {Folder}", fileName, subfolder);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete image {FileName} from {Folder}", fileName, subfolder);
        }
    }
}
