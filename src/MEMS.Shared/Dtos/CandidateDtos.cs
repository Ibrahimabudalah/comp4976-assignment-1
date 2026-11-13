using System.ComponentModel.DataAnnotations;

namespace MEMS.Shared.Dtos;

public class CandidateDto
{
    public int CandidateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string? PhotoFileName { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public int MunicipalityId { get; set; }
    public string MunicipalityName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
}

public class CreateCandidateDto
{
    [Required(ErrorMessage = "Candidate name is required.")]
    [StringLength(150, ErrorMessage = "Candidate name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Candidate biography is required.")]
    [StringLength(3000, ErrorMessage = "Biography cannot exceed 3000 characters.")]
    public string Profile { get; set; } = string.Empty;

    public string? PhotoFileName { get; set; }

    [Url(ErrorMessage = "Please enter a valid website URL.")]
    [StringLength(500)]
    public string? Website { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(254)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Position is required.")]
    public int PositionId { get; set; }
}

public class UpdateCandidateDto
{
    public int CandidateId { get; set; }

    [Required(ErrorMessage = "Candidate name is required.")]
    [StringLength(150, ErrorMessage = "Candidate name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Candidate biography is required.")]
    [StringLength(3000, ErrorMessage = "Biography cannot exceed 3000 characters.")]
    public string Profile { get; set; } = string.Empty;

    public string? PhotoFileName { get; set; }

    [Url(ErrorMessage = "Please enter a valid website URL.")]
    [StringLength(500)]
    public string? Website { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(254)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Position is required.")]
    public int PositionId { get; set; }
}

public class CandidateComparisonDto
{
    public List<CandidateDto> Candidates { get; set; } = new();
}
