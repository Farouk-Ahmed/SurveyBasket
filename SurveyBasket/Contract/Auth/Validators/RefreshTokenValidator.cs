using SurveyBasket.Contract.Auth.Request;

namespace SurveyBasket.Contract.Auth.Validators
{
    public class RefreshTokenValidator:AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenValidator() 
        {
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.RefreshToken).NotEmpty();


        }

    }
}
