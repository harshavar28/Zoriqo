using System.ComponentModel.DataAnnotations;

namespace Zoriqo.Application.Contracts.Skills;

public class SkillResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class UserSkillResponse
{
    // ID of this user's saved skill.
    public Guid Id { get; set; }

    // ID of the shared catalogue skill.
    public Guid SkillId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
    public bool IsCatalogueActive { get; set; }

    public long Version { get; set; }
}

public class AddUserSkillRequest : IValidatableObject
{
    public Guid? SkillId { get; set; }

    [StringLength(100)]
    public string? NewSkillName { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        var hasId = SkillId.HasValue;
        var hasName = !string.IsNullOrWhiteSpace(NewSkillName);

        if (hasId == hasName)
        {
            yield return new ValidationResult(
                "Provide either skillId or newSkillName, but not both.",
                new[] { nameof(SkillId), nameof(NewSkillName) });
        }

        if (hasId && SkillId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Select a valid catalogue skill.",
                new[] { nameof(SkillId) });
        }

        if (hasName && NewSkillName!.Trim().Length < 2)
        {
            yield return new ValidationResult(
                "The skill name must contain at least two characters.",
                new[] { nameof(NewSkillName) });
        }
    }
}

public class UpdateUserSkillRequest : IValidatableObject
{
    // Omit/null: leave unchanged.
    // Empty string: clear the description.
    [StringLength(1000)]
    public string? Description { get; set; }

    // Omit/null: leave unchanged.
    public bool? IsActive { get; set; }

    [Range(typeof(long), "1", "9223372036854775806")]
    public long Version { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Description is null && IsActive is null)
        {
            yield return new ValidationResult(
                "Provide a description or an active status.",
                new[] { nameof(Description), nameof(IsActive) });
        }
    }
}

public enum SkillOperationStatus
{
    Success,
    NotFound,
    Conflict,
    Invalid
}

public record SkillOperationResult(
    SkillOperationStatus Status,
    UserSkillResponse? Skill = null,
    string? Message = null);