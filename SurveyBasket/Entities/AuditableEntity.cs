namespace SurveyBasket.Entities
{
    public class AuditableEntity
    {
        public string CreatedById { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // fixed name to match navigation and EF conventions
        public string? UpdatedById { get; set; } 
        public DateTime? UpdatedOn { get; set; } 

        public AppUser CreatedBy { get; set; } = default!;
        public AppUser? UpdatedBy { get; set; }
    }
}
