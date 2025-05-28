namespace SurveyBasket.Contract.Poll.Response
{
    public record pollResponse (
         int Id,
          string Title,
            string Note ,
            bool IsPublished,
            DateTime StartsAt,
            DateTime EndsAt 
    );
    
}
