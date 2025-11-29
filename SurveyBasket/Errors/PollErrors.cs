using SurveyBasket.Abstractions;

namespace SurveyBasket.Errors
{
    public class PollErrors
    {
        public static readonly Error PollNotFound =
          new("Poll.NotFound", "Not Poll was Found With The Given ID.");
    }
}
