using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zoriqo.Application.Contracts.Skills;
using Zoriqo.Application.Interfaces;

namespace Zoriqo.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/skills")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skills;

    public SkillsController(ISkillService skills)
    {
        _skills = skills;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<SkillResponse>>> Search(
        [FromQuery, StringLength(100)] string? query,
        CancellationToken cancellationToken)
    {
        var result = await _skills.SearchAsync(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("/api/me/skills")]
    public async Task<ActionResult<List<UserSkillResponse>>> GetMine(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _skills.GetUserSkillsAsync(
            userId,
            includeInactive: true,
            cancellationToken: cancellationToken);

        if (result is null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("/api/profiles/{userId}/skills")]
    public async Task<ActionResult<List<UserSkillResponse>>> GetPublic(
        string userId,
        CancellationToken cancellationToken)
    {
        var result = await _skills.GetUserSkillsAsync(
            userId,
            includeInactive: false,
            cancellationToken: cancellationToken);

        if (result is null)
        {
            return NotFound(new
            {
                message = "Profile was not found."
            });
        }

        return Ok(result);
    }

    [HttpPost("/api/me/skills")]
    public async Task<ActionResult<UserSkillResponse>> Add(
        [FromBody] AddUserSkillRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _skills.AddAsync(
            userId,
            request,
            cancellationToken);

        return OperationResponse(result, created: true);
    }

    [HttpPatch("/api/me/skills/{id:guid}")]
    public async Task<ActionResult<UserSkillResponse>> Update(
        Guid id,
        [FromBody] UpdateUserSkillRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _skills.UpdateAsync(
            userId,
            id,
            request,
            cancellationToken);

        return OperationResponse(result, created: false);
    }

    private ActionResult<UserSkillResponse> OperationResponse(
        SkillOperationResult result,
        bool created)
    {
        switch (result.Status)
        {
            case SkillOperationStatus.Success:
                return StatusCode(
                    created
                        ? StatusCodes.Status201Created
                        : StatusCodes.Status200OK,
                    result.Skill);

            case SkillOperationStatus.NotFound:
                return NotFound(new
                {
                    message = result.Message
                });

            case SkillOperationStatus.Conflict:
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
}