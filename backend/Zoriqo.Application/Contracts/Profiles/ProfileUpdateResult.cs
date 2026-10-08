namespace Zoriqo.Application.Contracts.Profiles;

public enum ProfileUpdateStatus
{
    Success,
    NotFound,
    Conflict,
    Invalid
}

public record ProfileUpdateResult(
    ProfileUpdateStatus Status,
    ProfileResponse? Profile = null,
    string? Message = null);