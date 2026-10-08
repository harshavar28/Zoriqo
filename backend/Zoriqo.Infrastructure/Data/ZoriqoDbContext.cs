using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zoriqo.Domain.Entities;
using Zoriqo.Infrastructure.Data.Configurations;
using Zoriqo.Infrastructure.Identity;
using Zoriqo.Domain.Entities;
using Zoriqo.Infrastructure.Data.Configurations;

namespace Zoriqo.Infrastructure.Data;

public class ZoriqoDbContext : IdentityDbContext<ApplicationUser>
{
    public ZoriqoDbContext(
        DbContextOptions<ZoriqoDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<UserSkill> UserSkills => Set<UserSkill>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Existing account tables stay in this schema.
        builder.HasDefaultSchema("zoriqo_identity");

        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(x => x.FullName)
                .HasMaxLength(100)
                .IsRequired();

            user.Property(x => x.Email)
                .IsRequired();

            user.Property(x => x.NormalizedEmail)
                .IsRequired();

            user.HasIndex(x => x.NormalizedEmail)
                .IsUnique();
        });

        // Profile table explicitly uses the zoriqo schema.
        builder.ApplyConfiguration(new UserProfileConfiguration());
        builder.ApplyConfiguration(new SkillConfiguration());
        builder.ApplyConfiguration(new UserSkillConfiguration());
    }
}