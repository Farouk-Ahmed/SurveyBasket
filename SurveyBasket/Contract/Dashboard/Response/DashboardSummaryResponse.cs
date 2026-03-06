using SurveyBasket.Contract.Dashboard.Response;

namespace SurveyBasket.Contract.Dashboard.Response
{
    public record DashboardSummaryResponse(
        int TotalPolls,
        int PublishedPolls,
        int UnpublishedPolls,
        int DeletedPolls,
        IEnumerable<DashboardPollResponse> Polls
    );
}
