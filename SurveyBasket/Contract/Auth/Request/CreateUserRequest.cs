namespace SurveyBasket.Contract.Auth.Request
{
    public record CreateUserRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Role,   // "Admin" or "User"
        string? NationalId,
        string? MainMobile,
        string? AlternateMobile,
        string? MainAddress,
        string? AlternateAddress,
        DateTime? DateOfBirth,
        string? Gender
    );
}
