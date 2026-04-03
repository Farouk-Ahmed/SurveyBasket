namespace SurveyBasket.Application.DTOs.Dashboard;

public record DashboardSummaryResponse(int TotalPolls, int PublishedPolls, int UnpublishedPolls, int DeletedPolls, IEnumerable<DashboardPollResponse> Polls);
