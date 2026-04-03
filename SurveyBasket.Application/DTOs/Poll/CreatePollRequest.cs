namespace SurveyBasket.Application.DTOs.Poll;

public record CreatePollRequest(string Title, string Summray, bool IsPublished, DateTime StartsAt, DateTime EndsAt);
