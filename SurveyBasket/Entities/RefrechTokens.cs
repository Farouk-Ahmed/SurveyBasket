namespace SurveyBasket.Entities
{
    [Owned]
    public class RefrechTokens
    {
      
        public string Token { get; set; }   =string.Empty;
        public DateTime ExpirationOn { get; set; }
        public DateTime CreeatedOn { get; set; } =DateTime.UtcNow;
        public DateTime? RevokeOn { get; set; } 
        public bool IsExpired => DateTime.UtcNow >= ExpirationOn;
        public bool IsActive => RevokeOn is null && !IsExpired;






    }
}
