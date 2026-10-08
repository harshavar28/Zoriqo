namespace Zoriqo.Domain.Entities;

public class UserProfile
{
    public string UserId { get; set; } = string.Empty;

    // Personal statement shown below the profile name.
    public string? Headline { get; set; }

    public string? About { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? CountryCode { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public long Version { get; set; } = 1;
}