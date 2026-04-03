using FluentValidation;
using SurveyBasket.Application.DTOs.Auth;
using SurveyBasket.Domain.Abstractions;

namespace SurveyBasket.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        // UserName is auto-generated from FirstName + LastName; no rule needed.
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().WithMessage("Invalid email format.").MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.").MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.").MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.").MaximumLength(100);
        RuleFor(x => x.Role).NotEmpty().WithMessage("Role is required.")
            .Must(r => string.Equals(r, DefaultRoles.Admin, StringComparison.OrdinalIgnoreCase) || string.Equals(r, DefaultRoles.User, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Role must be Admin or User.");

        RuleFor(x => x.MainMobile).NotEmpty().WithMessage("Main mobile is required.").MaximumLength(20);
        RuleFor(x => x.MainAddress).NotEmpty().WithMessage("Main address is required.").MaximumLength(500);
        RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("Date of birth is required.");
        RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required.").MaximumLength(10);

        RuleFor(x => x.NationalId).MaximumLength(20);
        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National ID is required for clients.")
            .When(x => string.Equals(x.Role, DefaultRoles.User, StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.AlternateMobile).MaximumLength(20);
        RuleFor(x => x.AlternateAddress).MaximumLength(500);
    }
}
