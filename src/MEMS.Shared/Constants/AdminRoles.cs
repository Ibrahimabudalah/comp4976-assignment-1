namespace MEMS.Shared.Constants;

public static class AdminRoles
{
    public const string SuperAdmin = "Super Admin";
    public const string MunicipalityAdmin = "Municipality Admin";

    public static string ForMunicipality(string municipalityName) =>
        $"{municipalityName} Admin";
}
