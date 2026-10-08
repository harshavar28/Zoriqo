using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zoriqo.Application.Contracts.Profiles;
using Zoriqo.Application.Interfaces;

namespace Zoriqo.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profiles;

    public ProfilesController(IProfileService profiles)
    {
        _profiles = profiles;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileResponse>> GetMine(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var profile = await _profiles.GetAsync(
            userId,
            cancellationToken);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Profile was not found."
            });
        }

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<ProfileResponse>> UpdateMine(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _profiles.UpdateAsync(
            userId,
            request,
            cancellationToken);

        switch (result.Status)
        {
            case ProfileUpdateStatus.Success:
                return Ok(result.Profile);

            case ProfileUpdateStatus.NotFound:
                return NotFound(new
                {
                    message = result.Message
                });

            case ProfileUpdateStatus.Conflict:
                return Conflict(new
                {
                    message = result.Message
                });

            default:
                return BadRequest(new
                {
                    message = result.Message
                });
        }
    }

    [AllowAnonymous]
    [HttpGet("{userId}")]
    public async Task<ActionResult<ProfileResponse>> GetPublic(
        string userId,
        CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetAsync(
            userId,
            cancellationToken);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Profile was not found."
            });
        }

        return Ok(profile);
    }
}