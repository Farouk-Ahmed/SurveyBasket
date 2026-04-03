namespace SurveyBasket.Application.DTOs.Dashboard;

public class DashboardFilterRequest
{
    public bool? IsPublished { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public bool OnlyDeleted { get; set; } = false;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? SearchTerm { get; set; }
}
