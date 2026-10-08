using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zoriqo.Domain.Entities;

namespace Zoriqo.Infrastructure.Data.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("skills", "zoriqo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_skills_normalized_name");

        builder.HasData(
            Seed("11111111-1111-4111-8111-111111111101",
                "Interior painting"),
            Seed("11111111-1111-4111-8111-111111111102",
                "Wall finishing"),
            Seed("11111111-1111-4111-8111-111111111103",
                "Colour selection"),
            Seed("11111111-1111-4111-8111-111111111104",
                "Carpentry"),
            Seed("11111111-1111-4111-8111-111111111105",
                "Plumbing"),
            Seed("11111111-1111-4111-8111-111111111106",
                "Electrical work"),
            Seed("11111111-1111-4111-8111-111111111107",
                "Web development"),
            Seed("11111111-1111-4111-8111-111111111108",
                "Graphic design")
        );
    }

    private static Skill Seed(string id, string name)
    {
        // Fixed values keep migration generation deterministic.
        var timestamp = new DateTime(
            2026, 10, 8, 0, 0, 0, DateTimeKind.Utc);

        return new Skill
        {
            Id = Guid.Parse(id),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            IsActive = true,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };
    }
}