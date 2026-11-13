using System.ComponentModel.DataAnnotations;

namespace MEMS.Data.Entities;

public class Municipality
{
    public int MunicipalityId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string TenantSlug { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Province { get; set; } = "British Columbia";

    [Required]
    public DateTime ElectionDate { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public string? LogoFileName { get; set; }

    // Navigation properties
    public ICollection<Position> Positions { get; set; } = new List<Position>();
    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
