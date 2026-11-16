namespace MEMS.Public.Services;

public class ImageUrls(IConfiguration config)
{
    private const string Placeholder = "favicon.png";
    private readonly string _adminBase = (config["AdminBaseUrl"] ?? "").TrimEnd('/');

    // shortcut: images are served by MEMS.Admin's wwwroot; switch to API-served images if the partner moves uploads.
    public string Photo(string? file) => Build("candidates", file);
    public string Logo(string? file) => Build("logos", file);

    private string Build(string folder, string? file)
        => string.IsNullOrEmpty(file) ? Placeholder : $"{_adminBase}/uploads/{folder}/{Uri.EscapeDataString(file)}";
}
