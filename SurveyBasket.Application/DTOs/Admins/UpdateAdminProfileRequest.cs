using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Application.DTOs.Admins;

public class UpdateAdminProfileRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? NationalId { get; set; }
    [Required, MaxLength(500)]
    public string MainAddress { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? AlternateAddress { get; set; }
    [Required, MaxLength(20)]
    public string MainMobile { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? AlternateMobile { get; set; }
    [Required]
    public DateTime DateOfBirth { get; set; }
    [Required, MaxLength(10)]
    public string Gender { get; set; } = string.Empty;
}
