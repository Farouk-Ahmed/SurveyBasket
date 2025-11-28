using Microsoft.EntityFrameworkCore;

namespace SurveyBasket.Entities
{
    [Owned]
    public class RefrechTokens
    {
        // Primary key for the owned entity (used by the migrations)
        public int Id { get; set; }

        public string Token { get; set; } = string.Empty;
        public DateTime ExpirationOn { get; set; }
        public DateTime CreeatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? RevokeOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpirationOn;
        public bool IsActive => RevokeOn is null && !IsExpired;

    }
}
