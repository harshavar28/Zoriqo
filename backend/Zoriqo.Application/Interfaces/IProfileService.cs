using Zoriqo.Application.Contracts.Profiles;

namespace Zoriqo.Application.Interfaces;

public interface IProfileService
{
    Task<ProfileResponse?> GetAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<ProfileUpdateResult> UpdateAsync(
        string userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);
}