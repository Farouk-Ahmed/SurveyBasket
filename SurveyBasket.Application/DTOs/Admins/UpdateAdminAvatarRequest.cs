using Microsoft.AspNetCore.Http;

namespace SurveyBasket.Application.DTOs.Admins;

public class UpdateAdminAvatarRequest
{
    public IFormFile ProfilePicture { get; set; } = null!;
}
