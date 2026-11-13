using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MEMS.Data.Entities;

namespace MEMS.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Municipality> Municipalities { get; set; } = null!;
    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<Candidate> Candidates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Municipality constraints
        builder.Entity<Municipality>(entity =>
        {
            entity.HasKey(m => m.MunicipalityId);
            entity.HasIndex(m => m.Name).IsUnique();
            entity.HasIndex(m => m.TenantSlug).IsUnique();

            entity.HasMany(m => m.Positions)
                .WithOne(p => p.Municipality)
                .HasForeignKey(p => p.MunicipalityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(m => m.Candidates)
                .WithOne(c => c.Municipality)
                .HasForeignKey(c => c.MunicipalityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(m => m.Users)
                .WithOne(u => u.Municipality)
                .HasForeignKey(u => u.MunicipalityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Position constraints
        builder.Entity<Position>(entity =>
        {
            entity.HasKey(p => p.PositionId);
            entity.HasIndex(p => new { p.MunicipalityId, p.Name }).IsUnique();

            entity.HasMany(p => p.Candidates)
                .WithOne(c => c.Position)
                .HasForeignKey(c => c.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Candidate constraints
        builder.Entity<Candidate>(entity =>
        {
            entity.HasKey(c => c.CandidateId);
            entity.HasIndex(c => new { c.PositionId, c.Name });
            entity.HasIndex(c => c.MunicipalityId);
        });
    }
}
