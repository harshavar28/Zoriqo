using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zoriqo.Domain.Entities;
using Zoriqo.Infrastructure.Identity;

namespace Zoriqo.Infrastructure.Data.Configurations;

public class UserSkillConfiguration
    : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("user_skills", "zoriqo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.SkillId)
            .HasColumnName("skill_id");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Skill)
            .WithMany()
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        // A skill can be saved only once per user,
        // including inactive entries.
        builder.HasIndex(x => new { x.UserId, x.SkillId })
            .IsUnique()
            .HasDatabaseName("ux_user_skills_user_skill");

        // Supports later discovery of people with a particular skill.
        builder.HasIndex(x => new { x.SkillId, x.UserId })
            .HasFilter("is_active = true")
            .HasDatabaseName("ix_user_skills_active_skill_user");
    }
}