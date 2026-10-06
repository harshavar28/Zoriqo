using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Zoriqo.Application.Contracts.Auth;
using Zoriqo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Options;

namespace Zoriqo.Api.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("registerr")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var fullName = request.FullName.Trim();
        var email = request.Email.Trim();

        if (fullName.Length < 2)
        {
            return BadRequest(new
            {
                message = "Please enter a valid full name."
            });
        }

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        var user = new ApplicationUser
        {
            FullName = fullName,
            Email = email,

            // Email is the login identifier for now.
            UserName = email
        };

        IdentityResult result;

        try
        {
            // Identity hashes the password before storing it.
            result = await _userManager.CreateAsync(
                user,
                request.Password);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            // Handles simultaneous registrations with the same email.
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Unable to create account.",
                errors = result.Errors
                    .Select(error => error.Description)
                    .ToArray()
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Account created successfully.",
            user = new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email
            }
        });
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
    [FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(
            request.Email.Trim());

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Unable to sign in. Check your email and password."
            });
        }

        _signInManager.AuthenticationScheme =
            IdentityConstants.BearerScheme;

        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized(new
            {
                message =
                    "Unable to sign in. Check your details, or try again later."
            });
        }

        // Identity has already written the token response.
        return new EmptyResult();
    }
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            id = user.Id,
            fullName = user.FullName,
            email = user.Email
        });
    }
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
    [FromBody] RefreshTokenRequest request,
    [FromServices] IOptionsMonitor<BearerTokenOptions> options)
    {
        var bearerOptions = options.Get(
            IdentityConstants.BearerScheme);

        var ticket = bearerOptions.RefreshTokenProtector
            .Unprotect(request.RefreshToken);

        if (ticket?.Properties.ExpiresUtc is not { } expiresUtc ||
            expiresUtc <= DateTimeOffset.UtcNow)
        {
            return Unauthorized(new
            {
                message = "Your session expired. Please sign in again."
            });
        }

        var user = await _signInManager.ValidateSecurityStampAsync(
            ticket.Principal);

        if (user is null ||
            !await _signInManager.CanSignInAsync(user) ||
            await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new
            {
                message = "Please sign in again."
            });
        }

        var principal =
            await _signInManager.CreateUserPrincipalAsync(user);

        // Issues a fresh access token and refresh token.
        return SignIn(
            principal,
            IdentityConstants.BearerScheme);
    }
}