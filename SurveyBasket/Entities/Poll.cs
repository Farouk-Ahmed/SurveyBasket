namespace SurveyBasket.Entities
{
	public class Poll
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Summray { get; set; }
        public bool IsPublished { get; set; }
        public DateTime StartsAt { get; set; } = DateTime.Now;
        public DateTime EndsAt { get; set; } = DateTime.Now;

	}
}
