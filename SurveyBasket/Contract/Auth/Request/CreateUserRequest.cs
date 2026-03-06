namespace SurveyBasket.Contract.Auth.Request
{
    public record CreateUserRequest(
        string UserName,
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Role   // "Admin" or "User"
    );
}
