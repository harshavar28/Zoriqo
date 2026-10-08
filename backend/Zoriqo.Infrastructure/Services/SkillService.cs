using Microsoft.EntityFrameworkCore;
using Npgsql;
using Zoriqo.Application.Contracts.Skills;
using Zoriqo.Application.Interfaces;
using Zoriqo.Domain.Entities;
using Zoriqo.Infrastructure.Data;

namespace Zoriqo.Infrastructure.Services;

public class SkillService : ISkillService
{
    private readonly ZoriqoDbContext _db;

    public SkillService(ZoriqoDbContext db)
    {
        _db = db;
    }

    public async Task<List<SkillResponse>> SearchAsync(
        string? query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = Normalize(query);

        var skills = _db.Skills
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (normalizedQuery.Length > 0)
        {
            skills = skills.Where(x =>
                x.NormalizedName.Contains(normalizedQuery));
        }

        return await skills
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Take(50)
            .Select(x => new SkillResponse
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserSkillResponse>?> GetUserSkillsAsync(
        string userId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var userExists = await _db.Users
            .AnyAsync(x => x.Id == userId, cancellationToken);

        if (!userExists)
        {
            return null;
        }

        var skills = _db.UserSkills
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (!includeInactive)
        {
            skills = skills.Where(x =>
                x.IsActive && x.Skill.IsActive);
        }

        return await skills
            .OrderBy(x => x.Skill.Name)
            .ThenBy(x => x.Id)
            .Select(x => new UserSkillResponse
            {
                Id = x.Id,
                SkillId = x.SkillId,
                Name = x.Skill.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                IsCatalogueActive = x.Skill.IsActive,
                Version = x.Version
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SkillOperationResult> AddAsync(
    string userId,
    AddUserSkillRequest request,
    CancellationToken cancellationToken = default)
    {
        var hasId = request.SkillId.HasValue;
        var hasName = !string.IsNullOrWhiteSpace(request.NewSkillName);

        if (hasId == hasName)
        {
            return new(
                SkillOperationStatus.Invalid,
                Message: "Provide either skillId or newSkillName, but not both.");
        }

        if (hasId && request.SkillId == Guid.Empty)
        {
            return new(
                SkillOperationStatus.Invalid,
                Message: "Select a valid catalogue skill.");
        }

        string? displayName = null;
        string? normalizedName = null;

        if (hasName)
        {
            // Trim and collapse repeated spaces, tabs and line breaks.
            displayName = string.Join(
                " ",
                request.NewSkillName!.Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries));

            normalizedName = Normalize(displayName);

            if (displayName.Length < 2 ||
                displayName.Length > 100 ||
                normalizedName.Length > 100)
            {
                return new(
                    SkillOperationStatus.Invalid,
                    Message: "The skill name must contain 2 to 100 characters.");
            }
        }

        var userExists = await _db.Users.AnyAsync(
            x => x.Id == userId,
            cancellationToken);

        if (!userExists)
        {
            return new(
                SkillOperationStatus.NotFound,
                Message: "Account was not found.");
        }

        // Creating a catalogue entry and saving it to the user's
        // profile must either both succeed or both roll back.
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            Skill? skill;

            if (hasId)
            {
                skill = await _db.Skills.SingleOrDefaultAsync(
                    x => x.Id == request.SkillId!.Value,
                    cancellationToken);
            }
            else
            {
                var catalogueId = Guid.NewGuid();
                var catalogueCreatedAt = DateTime.UtcNow;

                // Values are passed as SQL parameters.
                // If another request already created this normalized name,
                // reuse that entry instead of creating a duplicate.
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"""
                INSERT INTO zoriqo.skills
                    (
                        id,
                        name,
                        normalized_name,
                        is_active,
                        created_at_utc,
                        updated_at_utc
                    )
                VALUES
                    (
                        {catalogueId},
                        {displayName},
                        {normalizedName},
                        TRUE,
                        {catalogueCreatedAt},
                        {catalogueCreatedAt}
                    )
                ON CONFLICT (normalized_name) DO NOTHING;
                """,
                    cancellationToken);

                skill = await _db.Skills.SingleOrDefaultAsync(
                    x => x.NormalizedName == normalizedName,
                    cancellationToken);
            }

            if (skill is null || !skill.IsActive)
            {
                return new(
                    SkillOperationStatus.Invalid,
                    Message: "This catalogue skill is unavailable.");
            }

            var existing = await _db.UserSkills
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.UserId == userId &&
                         x.SkillId == skill.Id,
                    cancellationToken);

            if (existing is not null)
            {
                return new(
                    SkillOperationStatus.Conflict,
                    Message: existing.IsActive
                        ? "This skill is already on your profile."
                        : "This skill is inactive. Reactivate your existing entry.");
            }

            var now = DateTime.UtcNow;

            var userSkill = new UserSkill
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SkillId = skill.Id,
                Skill = skill,
                Description = Clean(request.Description),
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                Version = 1
            };

            _db.UserSkills.Add(userSkill);

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new(
                SkillOperationStatus.Success,
                ToResponse(userSkill));
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "ux_user_skills_user_skill"
            })
        {
            await transaction.RollbackAsync(cancellationToken);

            return new(
                SkillOperationStatus.Conflict,
                Message: "This skill is already saved. Reload your skills.");
        }
    }

    public async Task<SkillOperationResult> UpdateAsync(
        string userId,
        Guid userSkillId,
        UpdateUserSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        // Ownership is checked as part of the query.
        var userSkill = await _db.UserSkills
            .Include(x => x.Skill)
            .SingleOrDefaultAsync(
                x => x.Id == userSkillId && x.UserId == userId,
                cancellationToken);

        if (userSkill is null)
        {
            return new(
                SkillOperationStatus.NotFound,
                Message: "Saved skill was not found.");
        }

        if (userSkill.Version != request.Version)
        {
            return ConflictResult();
        }

        if (request.IsActive == true && !userSkill.Skill.IsActive)
        {
            return new(
                SkillOperationStatus.Invalid,
                Message: "This catalogue skill is currently unavailable.");
        }

        if (request.Description is not null)
        {
            userSkill.Description = Clean(request.Description);
        }

        if (request.IsActive.HasValue)
        {
            userSkill.IsActive = request.IsActive.Value;
        }

        userSkill.UpdatedAtUtc = DateTime.UtcNow;
        userSkill.Version++;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ConflictResult();
        }

        return new(
            SkillOperationStatus.Success,
            ToResponse(userSkill));
    }

    private static UserSkillResponse ToResponse(UserSkill entry)
    {
        return new UserSkillResponse
        {
            Id = entry.Id,
            SkillId = entry.SkillId,
            Name = entry.Skill.Name,
            Description = entry.Description,
            IsActive = entry.IsActive,
            IsCatalogueActive = entry.Skill.IsActive,
            Version = entry.Version
        };
    }

    private static SkillOperationResult ConflictResult()
    {
        return new(
            SkillOperationStatus.Conflict,
            Message: "This skill changed. Reload your skills and try again.");
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string Normalize(string? value)
    {
        var words = (value ?? string.Empty).Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries);

        return string.Join(" ", words).ToUpperInvariant();
    }
}