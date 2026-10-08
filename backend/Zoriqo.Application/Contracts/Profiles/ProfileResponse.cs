namespace Zoriqo.Application.Contracts.Profiles;

public class ProfileResponse
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public string? Headline { get; set; }
    public string? About { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? CountryCode { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public long Version { get; set; }
}