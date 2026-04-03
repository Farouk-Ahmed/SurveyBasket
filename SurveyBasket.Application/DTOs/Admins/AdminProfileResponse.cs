namespace SurveyBasket.Application.DTOs.Admins;

public record AdminProfileResponse(int Id, string UserName, string FirstName, string LastName, string? NationalId, string Email, string MainMobile, string? AlternateMobile, string MainAddress, string? AlternateAddress, string Gender, DateTime DateOfBirth, string? ProfilePicturePath);
