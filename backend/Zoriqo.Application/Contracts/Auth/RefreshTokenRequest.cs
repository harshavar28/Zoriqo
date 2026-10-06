using System.ComponentModel.DataAnnotations;

namespace Zoriqo.Application.Contracts.Auth;

public class RefreshTokenRequest
{
    [Required]
    [StringLength(16384)]
    public string RefreshToken { get; set; } = string.Empty;
}