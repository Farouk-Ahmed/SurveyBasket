namespace SurveyBasket.Contract.Auth.Request
{
    public record RefreshRequest(
        string AccessToken,
        string RefreshToken
    );
}
