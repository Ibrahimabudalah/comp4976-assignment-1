using System.ComponentModel.DataAnnotations;

namespace MEMS.Data.Entities;

public class Position
{
    public int PositionId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100)]
    public int SeatsAvailable { get; set; } = 1;

    [Required]
    public int MunicipalityId { get; set; }

    // Navigation properties
    public Municipality Municipality { get; set; } = null!;
    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
}
