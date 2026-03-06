namespace SurveyBasket.Contract.Dashboard.Request
{
    public class DashboardFilterRequest
    {
        public bool? IsPublished { get; set; }
        public bool IncludeDeleted { get; set; } = false;
        public bool OnlyDeleted { get; set; } = false;
        public string? SortBy { get; set; }         // "title", "createdOn", "startsAt"
        public string? SortDirection { get; set; }  // "asc", "desc"
        public string? SearchTerm { get; set; }
    }
}
