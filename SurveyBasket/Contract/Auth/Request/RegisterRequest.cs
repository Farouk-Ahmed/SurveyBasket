namespace SurveyBasket.Contract.Auth.Request
{
    public record RegisterRequest(
        string UserName,
        string Email,
        string Password,
        string FirstName,
        string LastName
    );
}
