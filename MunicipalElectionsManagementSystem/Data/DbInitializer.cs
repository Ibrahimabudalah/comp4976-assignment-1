
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MunicipalElectionsManagementSystem.Models;

namespace MunicipalElectionsManagementSystem.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        //apply pending EF Core migrations
        await db.Database.MigrateAsync();

        //seed municipalities
        if (!await db.Municipalities.AnyAsync())
        {
            db.Municipalities.AddRange(
                new Municipality
                {
                    Name = "Pitt Meadows",
                    Province = "British Columbia",
                    ElectionDate = new DateTime(2026, 10, 17),
                    Description = "Municipal elections for Pitt Meadows."
                },
                new Municipality
                {
                    Name = "Maple Ridge",
                    Province = "British Columbia",
                    ElectionDate = new DateTime(2026, 10, 17),
                    Description = "Municipal elections for Maple Ridge."
                }
            );

            await db.SaveChangesAsync();
        }

        var pittMeadows = await db.Municipalities
            .FirstAsync(m => m.Name == "Pitt Meadows");

        var mapleRidge = await db.Municipalities
            .FirstAsync(m => m.Name == "Maple Ridge");

        //seed positions
        if (!await db.Positions.AnyAsync())
        {
            db.Positions.AddRange(
                new Position
                {
                    Name = "Mayor",
                    SeatsAvailable = 1,
                    MunicipalityId = pittMeadows.MunicipalityId
                },
                new Position
                {
                    Name = "Councilor",
                    SeatsAvailable = 6,
                    MunicipalityId = pittMeadows.MunicipalityId
                },
                new Position
                {
                    Name = "School Trustee",
                    SeatsAvailable = 2,
                    MunicipalityId = pittMeadows.MunicipalityId
                },
                new Position
                {
                    Name = "Mayor",
                    SeatsAvailable = 1,
                    MunicipalityId = mapleRidge.MunicipalityId
                },
                new Position
                {
                    Name = "Councilor",
                    SeatsAvailable = 6,
                    MunicipalityId = mapleRidge.MunicipalityId
                }
            );

            await db.SaveChangesAsync();
        }

        //seed sample candidates
        if (!await db.Candidates.AnyAsync())
        {
            var pittMayor = await db.Positions.FirstAsync(
                p => p.Name == "Mayor" &&
                     p.MunicipalityId == pittMeadows.MunicipalityId);

            var pittCouncil = await db.Positions.FirstAsync(
                p => p.Name == "Councilor" &&
                     p.MunicipalityId == pittMeadows.MunicipalityId);

            var mapleMayor = await db.Positions.FirstAsync(
                p => p.Name == "Mayor" &&
                     p.MunicipalityId == mapleRidge.MunicipalityId);

            db.Candidates.AddRange(
                new Candidate
                {
                    Name = "Alex Morgan",
                    Profile = "Focused on community development and local services.",
                    PositionId = pittMayor.PositionId
                },
                new Candidate
                {
                    Name = "Jordan Lee",
                    Profile = "Interested in transportation and affordable housing.",
                    PositionId = pittCouncil.PositionId
                },
                new Candidate
                {
                    Name = "Taylor Chen",
                    Profile = "Supports local businesses and community programs.",
                    PositionId = mapleMayor.PositionId
                }
            );

            await db.SaveChangesAsync();
        }

        //create required roles
        string[] roles = { "Super Admin", "Pitt Meadows Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                EnsureSuccess(result, $"Create role {role}");
            }
        }

        //seed users
        await CreateUserAsync(
            userManager,
            "super",
            "Super Admin",
            null,
            configuration["SeedUsers:SuperPassword"]);

        await CreateUserAsync(
            userManager,
            "pm",
            "Pitt Meadows Admin",
            pittMeadows.MunicipalityId,
            configuration["SeedUsers:PmPassword"]);
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string username,
        string role,
        int? municipalityId,
        string? password)
    {
        var user = await userManager.FindByNameAsync(username);

        if (user == null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    $"Seed password for {username} is missing.");
            }

            user = new ApplicationUser
            {
                UserName = username,
                MunicipalityId = municipalityId
            };

            var result = await userManager.CreateAsync(user, password);
            EnsureSuccess(result, $"Create user {username}");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var result = await userManager.AddToRoleAsync(user, role);
            EnsureSuccess(result, $"Assign role {role} to {username}");
        }
    }

    private static void EnsureSuccess(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(e => e.Description));

        throw new InvalidOperationException(
            $"{operation} failed: {errors}");
    }
}