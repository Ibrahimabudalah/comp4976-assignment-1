using System.ComponentModel.DataAnnotations;

namespace MEMS.Shared.Dtos;

public class PositionDto
{
    public int PositionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SeatsAvailable { get; set; }
    public int MunicipalityId { get; set; }
    public string MunicipalityName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public int CandidateCount { get; set; }
}

public class CreatePositionDto
{
    [Required(ErrorMessage = "Position name is required.")]
    [StringLength(100, ErrorMessage = "Position name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Seats available must be between 1 and 100.")]
    public int SeatsAvailable { get; set; } = 1;

    [Required(ErrorMessage = "Municipality is required.")]
    public int MunicipalityId { get; set; }
}

public class UpdatePositionDto
{
    public int PositionId { get; set; }

    [Required(ErrorMessage = "Position name is required.")]
    [StringLength(100, ErrorMessage = "Position name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Seats available must be between 1 and 100.")]
    public int SeatsAvailable { get; set; }

    [Required(ErrorMessage = "Municipality is required.")]
    public int MunicipalityId { get; set; }
}
