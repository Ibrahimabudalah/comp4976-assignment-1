namespace MEMS.Shared.Dtos;

public class VoterGuideItemDto
{
    public int CandidateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int PositionId { get; set; }
    public int SeatsAvailable { get; set; }
    public int MunicipalityId { get; set; }
    public string MunicipalityName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public string? PhotoFileName { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public class VoterBallotValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public static VoterBallotValidationResult Success() => new() { IsValid = true };
    public static VoterBallotValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
}
