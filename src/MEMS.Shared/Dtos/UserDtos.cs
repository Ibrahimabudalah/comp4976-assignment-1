using System.ComponentModel.DataAnnotations;

namespace MEMS.Shared.Dtos;

public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public int? MunicipalityId { get; set; }
    public string? MunicipalityName { get; set; }
    public string? TenantSlug { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class CreateAdminUserDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public int? MunicipalityId { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;
}

public class UpdateAdminUserDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FullName { get; set; }

    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    public int? MunicipalityId { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username is required.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
