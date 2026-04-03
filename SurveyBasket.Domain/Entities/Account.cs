namespace SurveyBasket.Domain.Entities;

/// <summary>
/// Single table for all users: profile + role (Client/Admin) + extra fields. Replaces separate Client/Admin tables.
/// </summary>
public class Account
{
    public int Id { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    /// <summary>Role: DefaultRoles.User (Client) or DefaultRoles.Admin.</summary>
    public string Role { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Extra profile fields (optional for Admin)
    public string? NationalId { get; set; }
    public string MainAddress { get; set; } = string.Empty;
    public string? AlternateAddress { get; set; }
    public string MainMobile { get; set; } = string.Empty;
    public string? AlternateMobile { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;

    public AppUser AppUser { get; set; } = default!;
    public ICollection<Poll> Polls { get; set; } = new List<Poll>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
