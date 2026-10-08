namespace Zoriqo.Domain.Entities;

public class Skill
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}