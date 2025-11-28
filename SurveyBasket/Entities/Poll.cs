namespace SurveyBasket.Entities
{
	public class Poll : AuditableEntity
    {
		public int Id { get; set; }
		public string Title { get; set; }  =string.Empty;
        public string Summray { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime StartsAt { get; set; } = DateTime.Now;
        public DateTime EndsAt { get; set; } = DateTime.Now;
     

    }
}
