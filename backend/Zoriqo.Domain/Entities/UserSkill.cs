namespace Zoriqo.Domain.Entities;

public class UserSkill
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public long Version { get; set; } = 1;
}