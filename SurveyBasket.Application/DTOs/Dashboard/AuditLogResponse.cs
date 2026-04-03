namespace SurveyBasket.Application.DTOs.Dashboard;

public record AuditLogResponse(int Id, int PollId, string PollTitle, string Action, string PerformedByName, DateTime PerformedOn, string? Details, string? DeletionReason = null);
