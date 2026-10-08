using Zoriqo.Application.Contracts.Skills;

namespace Zoriqo.Application.Interfaces;

public interface ISkillService
{
    Task<List<SkillResponse>> SearchAsync(
        string? query,
        CancellationToken cancellationToken = default);

    Task<List<UserSkillResponse>?> GetUserSkillsAsync(
        string userId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<SkillOperationResult> AddAsync(
        string userId,
        AddUserSkillRequest request,
        CancellationToken cancellationToken = default);

    Task<SkillOperationResult> UpdateAsync(
        string userId,
        Guid userSkillId,
        UpdateUserSkillRequest request,
        CancellationToken cancellationToken = default);
}