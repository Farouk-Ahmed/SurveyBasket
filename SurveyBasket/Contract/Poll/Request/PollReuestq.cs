using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Contract.Poll.Request
{
    public record PollReuestq
        (
          /*[Required(ErrorMessage ="Plese Enter The titel")]*/
          //[AllowedValues("OLD","NEW",ErrorMessage ="Jest Enter OLD Or NEW Only" ) ]
            string Title,
          //[Length(10,20)]
            string Summray ,
            bool IsPublished,
            DateTime StartsAt,
            DateTime EndsAt 

        );
    
}
