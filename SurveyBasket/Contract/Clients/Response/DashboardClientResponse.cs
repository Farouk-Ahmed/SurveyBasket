namespace SurveyBasket.Contract.Clients.Response
{
    public record DashboardClientResponse(
        int Id,
        string FirstName,
        string LastName,
        string NationalId,
        string Email,
        string MainMobile,
        string? AlternateMobile,
        string MainAddress,
        string? AlternateAddress,
        string Gender,
        DateTime DateOfBirth,
        string? ProfilePictureUrl,
        string AppUserId,
        DateTime RegisteredOn
    );
}
