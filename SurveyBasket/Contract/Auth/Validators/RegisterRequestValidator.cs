using SurveyBasket.Contract.Auth.Request;

namespace SurveyBasket.Contract.Auth.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100);

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("National ID is required.")
                .MaximumLength(20);

            RuleFor(x => x.MainAddress)
                .NotEmpty().WithMessage("Main address is required.")
                .MaximumLength(500);

            RuleFor(x => x.MainMobile)
                .NotEmpty().WithMessage("Main mobile number is required.")
                .MaximumLength(20);

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required.");
        }
    }
}
