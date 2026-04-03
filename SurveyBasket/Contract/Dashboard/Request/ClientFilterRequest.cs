namespace SurveyBasket.Contract.Dashboard.Request
{
    public class ClientFilterRequest
    {
        public string? Email { get; set; }
        public string? SearchTerm { get; set; } // Can search by Name or NationalId
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } // E.g., name, registeredOn, email
        public string? SortDirection { get; set; } // asc or desc
    }
}
