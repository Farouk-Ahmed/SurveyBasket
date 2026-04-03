using FluentValidation;
using SurveyBasket.Application.DTOs.Clients;

namespace SurveyBasket.Application.Validators;

public class UpdateClientProfileRequestValidator : AbstractValidator<UpdateClientProfileRequest>
{
    public UpdateClientProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.").MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.").MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");
        RuleFor(x => x.MainAddress).NotEmpty().WithMessage("Main address is required.").MaximumLength(500).WithMessage("Main address cannot exceed 500 characters.");
        RuleFor(x => x.AlternateAddress).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.AlternateAddress)).WithMessage("Alternate address cannot exceed 500 characters.");
        RuleFor(x => x.MainMobile).NotEmpty().WithMessage("Main mobile is required.").MaximumLength(20).WithMessage("Main mobile cannot exceed 20 characters.");
        RuleFor(x => x.AlternateMobile).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.AlternateMobile)).WithMessage("Alternate mobile cannot exceed 20 characters.");
        RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required.").MaximumLength(10).WithMessage("Gender cannot exceed 10 characters.");
        RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("Date of birth is required.").LessThan(DateTime.UtcNow).WithMessage("Date of birth cannot be in the future.");
    }
}
