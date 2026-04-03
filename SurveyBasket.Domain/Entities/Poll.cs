namespace SurveyBasket.Domain.Entities;

public class Poll : AuditableEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summray { get; set; } = string.Empty; // Note: DB column name
    public bool IsPublished { get; set; }
    public DateTime StartsAt { get; set; } = DateTime.Now;
    public DateTime EndsAt { get; set; } = DateTime.Now;
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
