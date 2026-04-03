using Microsoft.AspNetCore.Http;

namespace SurveyBasket.Contract.Clients.Request
{
    public class UpdateClientAvatarRequest
    {
        public IFormFile ProfilePicture { get; set; } = null!;
    }
}
