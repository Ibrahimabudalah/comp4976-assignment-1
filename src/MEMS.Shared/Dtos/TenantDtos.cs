using System.ComponentModel.DataAnnotations;

namespace MEMS.Shared.Dtos;

public class MunicipalityDto
{
    public int MunicipalityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public DateTime ElectionDate { get; set; }
    public string? Description { get; set; }
    public string? LogoFileName { get; set; }
    public int PositionCount { get; set; }
    public int CandidateCount { get; set; }
}

public class CreateMunicipalityDto
{
    [Required(ErrorMessage = "Municipality name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tenant slug is required for URL routing (e.g. 'burnaby').")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Tenant slug can only contain lowercase letters, numbers, and hyphens.")]
    [StringLength(50, ErrorMessage = "Slug cannot exceed 50 characters.")]
    public string TenantSlug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Province is required.")]
    [StringLength(100)]
    public string Province { get; set; } = "British Columbia";

    [Required(ErrorMessage = "Election date is required.")]
    [DataType(DataType.Date)]
    public DateTime ElectionDate { get; set; } = new DateTime(2026, 10, 17);

    [StringLength(2000)]
    public string? Description { get; set; }

    public string? LogoFileName { get; set; }

    // Resolves A1 deficiency: option to create an initial municipality admin during municipality creation
    public bool CreateInitialAdmin { get; set; } = false;

    public string? AdminUserName { get; set; }

    public string? AdminFullName { get; set; }

    [DataType(DataType.Password)]
    public string? AdminPassword { get; set; }
}

public class UpdateMunicipalityDto
{
    public int MunicipalityId { get; set; }

    [Required(ErrorMessage = "Municipality name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tenant slug is required for URL routing (e.g. 'burnaby').")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Tenant slug can only contain lowercase letters, numbers, and hyphens.")]
    [StringLength(50, ErrorMessage = "Slug cannot exceed 50 characters.")]
    public string TenantSlug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Province is required.")]
    [StringLength(100)]
    public string Province { get; set; } = "British Columbia";

    [Required(ErrorMessage = "Election date is required.")]
    [DataType(DataType.Date)]
    public DateTime ElectionDate { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public string? LogoFileName { get; set; }
}
