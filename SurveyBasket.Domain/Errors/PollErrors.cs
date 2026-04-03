using SurveyBasket.Domain.Abstractions;

namespace SurveyBasket.Domain.Errors;

public static class PollErrors
{
    public static readonly Error PollNotFound = new("Poll.NotFound", "Not Poll was Found With The Given ID.");
    public static readonly Error DuplicateTitle = new("Poll.DuplicateTitle", "A poll with this title already exists. Please choose a different title.");
}
