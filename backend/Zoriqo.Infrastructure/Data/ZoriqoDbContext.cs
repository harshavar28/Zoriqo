using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zoriqo.Infrastructure.Identity;

namespace Zoriqo.Infrastructure.Data;

public class ZoriqoDbContext : IdentityDbContext<ApplicationUser>
{
    public ZoriqoDbContext(
        DbContextOptions<ZoriqoDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Keep account tables in their own database schema.
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
    }
}