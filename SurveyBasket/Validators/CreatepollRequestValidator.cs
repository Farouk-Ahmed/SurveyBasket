namespace SurveyBasket.Validators
{
    public class CreatepollRequestValidator : AbstractValidator<PollReuestq>
    {
        public CreatepollRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.").Length(10, 20).WithMessage("Title must be between 10 and 20 characters."); 

            RuleFor(x => x.Summray).NotEmpty().WithMessage("Description is required.");
            RuleFor(x => x.StartsAt).NotEmpty().
				GreaterThanOrEqualTo(DateTime.Today);
            RuleFor(x => x).Must(HasValidTime).WithName(nameof(PollReuestq.EndsAt)).WithMessage("Start date must be today or later.");
		}
        private bool HasValidTime(PollReuestq poll)
        {
            return  poll.EndsAt>=poll.StartsAt;
        }

    }
}

