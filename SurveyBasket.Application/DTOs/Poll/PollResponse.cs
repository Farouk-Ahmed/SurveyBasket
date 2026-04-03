using SurveyBasket.Application.DTOs.Files;

namespace SurveyBasket.Application.DTOs.Poll;

public record PollResponse(int Id, string Title, string Note, bool IsPublished, DateTime StartsAt, DateTime EndsAt, IEnumerable<FileResponse> Images);
