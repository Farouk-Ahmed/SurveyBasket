using SurveyBasket.Contract.Poll.Request;

namespace SurveyBasket.Contract.Poll.Validators
{
    public class CreatepollRequestValidator : AbstractValidator<PollReuestq>
    {
        public CreatepollRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.").Length(5, 20).WithMessage("Title must be between 5 and 20 characters."); 

            RuleFor(x => x.Summray).NotEmpty().WithMessage("Description is required.");
            RuleFor(x => x.StartsAt).NotEmpty()
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past.");
                
            RuleFor(x => x).Must(HasValidTimeFrame).WithName(nameof(PollReuestq.EndsAt))
                .WithMessage("End date must be after Start date and cannot exceed 3 months from the Start date.");
        }

        private bool HasValidTimeFrame(PollReuestq poll)
        {
            if (poll.EndsAt < poll.StartsAt) return false;
            
            // End date cannot be more than 3 months from start date
            return poll.EndsAt <= poll.StartsAt.AddMonths(3);
        }

    }
}

