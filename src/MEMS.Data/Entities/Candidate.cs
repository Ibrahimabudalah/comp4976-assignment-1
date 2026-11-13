using System.ComponentModel.DataAnnotations;

namespace MEMS.Data.Entities;

public class Candidate
{
    public int CandidateId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(3000)]
    public string Profile { get; set; } = string.Empty;

    public string? PhotoFileName { get; set; }

    [StringLength(500)]
    public string? Website { get; set; }

    [StringLength(254)]
    public string? Email { get; set; }

    [Required]
    public int PositionId { get; set; }

    [Required]
    public int MunicipalityId { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Position Position { get; set; } = null!;
    public Municipality Municipality { get; set; } = null!;
}
