

namespace SurveyBasket.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PollsController(IPoll_serveis poll_service) : ControllerBase
	{
		private readonly IPoll_serveis _poll_service = poll_service;

		[HttpGet]
		[Route("GetPolls")]
		public IActionResult GetPolls()
		{
			return Ok(_poll_service.GetPolls());
		}
		[HttpGet]
		[Route("{id}")]
		public IActionResult GetbyID(int id)
		{

			var pool = _poll_service.GetPollById(id);
			return pool is null ? NotFound() : Ok(pool);


		}
		[HttpPost]
		[Route("addpool")]
		public IActionResult add(Poll request)
		{
			var newpool = _poll_service.add(request);
			return CreatedAtAction(nameof(GetPolls), new { id = newpool.Id }, newpool);
		}
		[HttpPut("id")]
		public IActionResult update(Poll request, int id)
		{
			var currentPoll = _poll_service.update(request, id);
			if (!currentPoll)
				return NotFound();
			return NoContent();
		}
		[HttpDelete("{id}")]
		public IActionResult Deleted(int id)
		{
			var curantpoll = _poll_service.Dlete(id);
			if(!curantpoll)
                return NotFound();
            return NoContent();
        }


	}
}
