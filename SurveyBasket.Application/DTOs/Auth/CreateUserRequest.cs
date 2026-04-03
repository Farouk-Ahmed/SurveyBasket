using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Application.DTOs.Auth;

/// <summary>Username is auto-generated from FirstName + LastName; do not send.</summary>
public class CreateUserRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Role { get; set; } = string.Empty; // Admin | User

    [MaxLength(20)]
    public string? NationalId { get; set; }
    [Required, MaxLength(20)]
    public string MainMobile { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? AlternateMobile { get; set; }
    [Required, MaxLength(500)]
    public string MainAddress { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? AlternateAddress { get; set; }
    [Required]
    public DateTime DateOfBirth { get; set; }
    [Required, MaxLength(10)]
    public string Gender { get; set; } = string.Empty;
}
