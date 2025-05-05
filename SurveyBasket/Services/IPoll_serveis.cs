namespace SurveyBasket.Services
{
    public interface IPoll_serveis
    {
        IEnumerable<Poll> GetPolls();
        Poll? GetPollById(int id);
        Poll add(Poll poll);
        bool update(Poll poll, int id);
        bool Dlete(int id);
    }
}
