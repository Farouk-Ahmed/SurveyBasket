using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyBasket.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpirationOn { get; set; }
    [Column("CreeatedOn")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? RevokeOn { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpirationOn;
    public bool IsActive => RevokeOn is null && !IsExpired;
}
