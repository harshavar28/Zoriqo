using Microsoft.EntityFrameworkCore;
using Zoriqo.Application.Contracts.Profiles;
using Zoriqo.Application.Interfaces;
using Zoriqo.Infrastructure.Data;

namespace Zoriqo.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly ZoriqoDbContext _db;

    public ProfileService(ZoriqoDbContext db)
    {
        _db = db;
    }

    public Task<ProfileResponse?> GetAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return (
            from profile in _db.UserProfiles.AsNoTracking()
            join user in _db.Users.AsNoTracking()
                on profile.UserId equals user.Id
            where profile.UserId == userId
            select new ProfileResponse
            {
                UserId = profile.UserId,
                FullName = user.FullName,
                Headline = profile.Headline,
                About = profile.About,
                City = profile.City,
                Region = profile.Region,
                CountryCode = profile.CountryCode,
                CreatedAtUtc = profile.CreatedAtUtc,
                UpdatedAtUtc = profile.UpdatedAtUtc,
                Version = profile.Version
            }
        ).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ProfileUpdateResult> UpdateAsync(
        string userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var fullName = request.FullName.Trim();

        if (fullName.Length < 2)
        {
            return new(
                ProfileUpdateStatus.Invalid,
                Message: "Full name must contain at least two characters.");
        }

        var profile = await _db.UserProfiles
            .SingleOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (profile is null)
        {
            return new(
                ProfileUpdateStatus.NotFound,
                Message: "Profile was not found.");
        }

        if (profile.Version != request.Version)
        {
            return ConflictResult();
        }

        var user = await _db.Users
            .SingleAsync(
                x => x.Id == userId,
                cancellationToken);

        user.FullName = fullName;

        profile.Headline = Clean(request.Headline);
        profile.About = Clean(request.About);
        profile.City = Clean(request.City);
        profile.Region = Clean(request.Region);

        profile.CountryCode =
            Clean(request.CountryCode)?.ToUpperInvariant();

        profile.UpdatedAtUtc = DateTime.UtcNow;
        profile.Version++;

        try
        {
            // Saves the name and profile changes together.
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ConflictResult();
        }

        return new(
            ProfileUpdateStatus.Success,
            new ProfileResponse
            {
                UserId = profile.UserId,
                FullName = user.FullName,
                Headline = profile.Headline,
                About = profile.About,
                City = profile.City,
                Region = profile.Region,
                CountryCode = profile.CountryCode,
                CreatedAtUtc = profile.CreatedAtUtc,
                UpdatedAtUtc = profile.UpdatedAtUtc,
                Version = profile.Version
            });
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static ProfileUpdateResult ConflictResult()
    {
        return new(
            ProfileUpdateStatus.Conflict,
            Message: "Your profile changed. Reload it and try again.");
    }
}