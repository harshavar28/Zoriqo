using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zoriqo.Domain.Entities;
using Zoriqo.Infrastructure.Identity;

namespace Zoriqo.Infrastructure.Data.Configurations;

public class UserProfileConfiguration
    : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles", "zoriqo");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .ValueGeneratedNever();

        builder.Property(x => x.Headline)
            .HasColumnName("headline")
            .HasMaxLength(160);

        builder.Property(x => x.About)
            .HasColumnName("about")
            .HasMaxLength(2000);

        builder.Property(x => x.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(x => x.Region)
            .HasColumnName("region")
            .HasMaxLength(100);

        builder.Property(x => x.CountryCode)
            .HasColumnName("country_code")
            .HasMaxLength(2);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .HasDefaultValue(1L)
            .IsConcurrencyToken();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}