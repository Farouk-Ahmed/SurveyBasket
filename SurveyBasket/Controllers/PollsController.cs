using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Poll.Request;
using SurveyBasket.Contract.Poll.Response;
using static MassTransit.ValidationResultExtensions;

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
            var result = await _poll_service.GetPollByIdAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
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
            var Ruselt = await _poll_service.UpDateAsync(request, id, cancellationToken);
                return Ruselt.IsSuccess ? NoContent() : NotFound(Ruselt.Error);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deleted(int id, CancellationToken cancellationToken)
        {
            var curantpoll = await _poll_service.DeleteAsync(id, cancellationToken);
            if (!curantpoll)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> TogglePublish(int id, CancellationToken cancellationToken)
        {
            var Ruselt = await _poll_service.TogglePublishAsync(id, cancellationToken);
            return Ruselt.IsSuccess ? NoContent() : NotFound(Ruselt.Error);

        }
    }
}
