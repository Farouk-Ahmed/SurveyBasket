namespace SurveyBasket.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string PerformedById { get; set; } = string.Empty;
        public DateTime PerformedOn { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }

        // Navigation properties
        public Poll Poll { get; set; } = default!;
        public AppUser PerformedBy { get; set; } = default!;
    }
}
