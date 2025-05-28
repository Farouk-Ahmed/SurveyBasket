

using SurveyBasket.Contract.Auth.Request;

namespace SurveyBasket.Contract.Poll.Validators
{
    public class AuthRequestValidator : AbstractValidator<AuthRequest>
    {
        public AuthRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.").EmailAddress();

            RuleFor(x => x.Password).NotEmpty().WithMessage("Passsword is required.");
 
        }
      

    }
}

