namespace SurveyBasket.Contract.Auth.Response
{
    public record CreateUserResponse(
        string Id,
        string? Email,
        string FirstName,
        string LastName,
        string Token,
        int ExpireIn,
        string RefreshToken,
        IList<string> Roles
    );
}
