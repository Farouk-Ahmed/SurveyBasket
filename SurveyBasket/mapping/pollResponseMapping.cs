using SurveyBasket.Contract.Poll.Response;

namespace SurveyBasket.mapping
{
    public class pollResponseMapping:IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Poll, pollResponse>()
                .Map(des => des.Note, src => src.Summray);



        }
    }
}
