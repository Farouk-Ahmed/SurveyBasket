namespace SurveyBasket.Domain.Entities;

public class Attachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedById { get; set; } = string.Empty;
    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
    public int? PollId { get; set; }
    public int AccountId { get; set; }
    public AppUser UploadedBy { get; set; } = default!;
    public Poll? Poll { get; set; }
    public Account Account { get; set; } = default!;
}
