
using Microsoft.AspNetCore.Http.HttpResults;

namespace SurveyBasket.Services
{
    public class Poll_serviceis:IPoll_serveis
    {
        private readonly List<Poll> _polls = new List<Poll>
        {
            new Poll { Id = 1, Title = "poll 1", Description="my first poll" }
        };

        public IEnumerable<Poll> GetPolls() => _polls;


        public Poll? GetPollById(int id) => _polls.FirstOrDefault(p => p.Id == id);

        public Poll add(Poll poll)
        {
            poll.Id=_polls.Count+1;
            _polls.Add(poll);
            return poll;
        }

        public bool update(Poll poll, int id)
        {
            var createdPoll = GetPollById(id);
            if(createdPoll is null)
                return false;
            createdPoll.Title = poll.Title;
            createdPoll.Description = poll.Description;
            return true;
        }

        public bool Dlete(int id)
        {
            var caruntpoll = GetPollById(id);
            if (caruntpoll is null)
                return false;
            _polls.Remove(caruntpoll);
            return true;


        }
    }
}
