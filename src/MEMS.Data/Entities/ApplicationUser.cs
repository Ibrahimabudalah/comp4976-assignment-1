using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MEMS.Data.Entities;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string? FullName { get; set; }

    public int? MunicipalityId { get; set; }

    // Navigation property
    public Municipality? Municipality { get; set; }
}
