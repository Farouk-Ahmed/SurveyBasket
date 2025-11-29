using SurveyBasket.Abstractions;

namespace SurveyBasket.Errors
{
    public class UserErrors
    {
        public static readonly Error invalidCredentials=
            new ("User.InvalidCredentials", "Invalid Email or Password. Try again.");
    }
}
