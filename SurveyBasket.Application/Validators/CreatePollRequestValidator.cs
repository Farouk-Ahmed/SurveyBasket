using FluentValidation;
using SurveyBasket.Application.DTOs.Poll;

namespace SurveyBasket.Application.Validators;

public class CreatePollRequestValidator : AbstractValidator<CreatePollRequest>
{
    public CreatePollRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.").Length(5, 20).WithMessage("Title must be between 5 and 20 characters.");
        RuleFor(x => x.Summray).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.StartsAt).NotEmpty().GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past.");
        RuleFor(x => x).Must(HasValidTimeFrame).WithName(nameof(CreatePollRequest.EndsAt)).WithMessage("End date must be after Start date and cannot exceed 3 months from the Start date.");
    }

    private static bool HasValidTimeFrame(CreatePollRequest poll) => poll.EndsAt >= poll.StartsAt && poll.EndsAt <= poll.StartsAt.AddMonths(3);
}
