using Microsoft.AspNetCore.Http;

namespace SurveyBasket.Application.DTOs.Clients;

public class UpdateClientAvatarRequest
{
    public IFormFile ProfilePicture { get; set; } = null!;
}
