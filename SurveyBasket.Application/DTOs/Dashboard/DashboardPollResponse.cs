using SurveyBasket.Application.DTOs.Files;

namespace SurveyBasket.Application.DTOs.Dashboard;

public record DashboardPollResponse(int Id, string Title, string Summary, bool IsPublished, DateTime StartsAt, DateTime EndsAt, string CreatedByName, string? UpdatedByName, DateTime CreatedOn, DateTime? UpdatedOn, bool IsDeleted, string? DeletedByName, DateTime? DeletedOn, string? DeletionReason, IEnumerable<FileResponse> Images);
