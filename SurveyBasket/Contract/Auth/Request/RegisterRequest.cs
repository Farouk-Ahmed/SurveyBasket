using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Contract.Auth.Request
{
    public class RegisterRequest
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string NationalId { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

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

        public IFormFile? ProfilePicture { get; set; }
    }
}
