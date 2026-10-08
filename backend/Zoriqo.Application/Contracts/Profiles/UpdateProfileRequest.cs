using System.ComponentModel.DataAnnotations;

namespace Zoriqo.Application.Contracts.Profiles;

public class UpdateProfileRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    // UI label: Personal statement.
    [StringLength(160)]
    public string? Headline { get; set; }

    [StringLength(2000)]
    public string? About { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Region { get; set; }

    [RegularExpression(
        "^[A-Za-z]{2}$",
        ErrorMessage = "Use a two-letter country code, such as IN.")]
    public string? CountryCode { get; set; }

    [Range(typeof(long), "1", "9223372036854775806")]
    public long Version { get; set; }
}