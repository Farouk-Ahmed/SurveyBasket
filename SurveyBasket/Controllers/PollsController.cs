using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Contract.Poll.Request;
using SurveyBasket.Contract.Poll.Response;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PollsController : ControllerBase
    {
        private readonly IPoll_serveis _poll_service;
        public PollsController(IPoll_serveis poll_service)
        {
            _poll_service = poll_service;
        }

        [HttpGet]
        [Route("GetPolls")]
        public async Task<IActionResult> GetPolls(CancellationToken cancellationToken)
        {
            var polls = await _poll_service.GetPollsAsync(cancellationToken);
            var res = polls.Adapt<IEnumerable<pollResponse>>();
            return Ok(res);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetbyIDAsync(int id, CancellationToken cancellationToken)
        {
            var pool = await _poll_service.GetPollByIdAsync(id, cancellationToken);
            if (pool is null)
                return NotFound();
            var response = pool.Adapt<pollResponse>();
            return Ok(response);
        }

        [HttpPost]
        [Route("Addpool")]
       
        public async Task<IActionResult> Add([FromBody] PollReuestq request, CancellationToken cancellationToken)
        {
            var newpool = await _poll_service.AddAsync(request.Adapt<Poll>(), cancellationToken);
            return CreatedAtAction(nameof(GetPolls), new { id = newpool.Id }, newpool.Adapt<pollResponse>());
        }

        [HttpPut("id")]
     
        public async Task<IActionResult> UpDate([FromBody] PollReuestq request, int id, CancellationToken cancellationToken)
        {
            var currentPoll = await _poll_service.UpDateAsync(request.Adapt<Poll>(), id, cancellationToken);
            if (!currentPoll)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deleted(int id, CancellationToken cancellationToken)
        {
            var curantpoll = await _poll_service.DeleteAsync(id, cancellationToken);
            if (!curantpoll)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{id}ToggelPublish")]
        public async Task<IActionResult> TogglePublish(int id, CancellationToken cancellationToken)
        {
            var changed = await _poll_service.TogglePublishAsync(id, cancellationToken);
            if (!changed)
                return NotFound();

            // retrieve updated poll to show the new value
            var updated = await _poll_service.GetPollByIdAsync(id, cancellationToken);
            if (updated is null)
                return Ok("Change ok, but failed to retrieve updated state.");

            var isPublished = updated.IsPublished;
            var message = $"Poll publish state changed to: {isPublished} ({(isPublished ? "Published" : "Unpublished")})";
            return Ok(message);
        }
    }
}
