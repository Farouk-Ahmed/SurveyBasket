namespace SurveyBasket.Domain.Entities;

public class AuditableEntity
{
    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? UpdatedById { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public AppUser CreatedBy { get; set; } = default!;
    public AppUser? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeletedById { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletionReason { get; set; }
    public AppUser? DeletedBy { get; set; }
}
