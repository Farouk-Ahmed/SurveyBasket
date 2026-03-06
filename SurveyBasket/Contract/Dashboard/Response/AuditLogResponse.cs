namespace SurveyBasket.Contract.Dashboard.Response
{
    public record AuditLogResponse(
        int Id,
        int PollId,
        string PollTitle,
        string Action,
        string PerformedByName,
        DateTime PerformedOn,
        string? Details
    );
}
