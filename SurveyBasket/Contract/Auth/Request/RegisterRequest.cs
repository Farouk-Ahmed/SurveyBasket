namespace SurveyBasket.Contract.Auth.Request
{
    public record RegisterRequest(
        string Email,
        string Password,
        string ConfirmPassword
    );
}
