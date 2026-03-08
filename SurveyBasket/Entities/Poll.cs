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

        // Navigation
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        public int ClientId { get; set; }
        public Client Client { get; set; } = default!;

    }
}
