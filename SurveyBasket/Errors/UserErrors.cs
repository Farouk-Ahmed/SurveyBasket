using SurveyBasket.Abstractions;

namespace SurveyBasket.Errors
{
    public class UserErrors
    {
        public static readonly Error invalidCredentials=
            new ("User.InvalidCredentials", "Invalid Email or Password. Try again.");

        public static readonly Error InvalidRole =
            new("User.InvalidRole", "Role must be 'Admin' or 'User'.");

        public static readonly Error RegistrationFailed =
            new("User.RegistrationFailed", "Registration failed. Check your data or try a different username/email.");
    }
}
