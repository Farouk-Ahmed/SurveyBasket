using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Application.DTOs.Poll;
using SurveyBasket.Application.DTOs.Files;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Entities;

namespace SurveyBasket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PollsController(IPollService pollService) : ControllerBase
{
    [HttpGet]
    [HttpGet("GetPolls")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPolls(CancellationToken cancellationToken)
    {
        var polls = await pollService.GetPollsAsync(cancellationToken);
        var scheme = Request.Scheme ?? "http";
        var host = Request.Host.Value ?? "localhost";
        var res = polls.Select(p => new PollResponse(p.Id, p.Title, p.Summray ?? "", p.IsPublished, p.StartsAt, p.EndsAt,
            (p.Attachments ?? Array.Empty<Attachment>()).Select(a => new FileResponse(a.Id, a.FileName ?? "", string.IsNullOrEmpty(a.StoredPath) ? $"{scheme}://{host}" : $"{scheme}://{host}/{a.StoredPath}", a.ContentType ?? "", a.FileSize, $"{a.UploadedBy?.FirstName} {a.UploadedBy?.LastName}".Trim(), a.UploadedById, a.UploadedOn, a.PollId))));
        return Ok(res);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await pollService.GetPollByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("Addpool")]
    public async Task<IActionResult> Add([FromBody] CreatePollRequest request, CancellationToken cancellationToken)
    {
        var result = await pollService.AddAsync(request.Adapt<Poll>(), cancellationToken);
        if (!result.IsSuccess)
            return Conflict(new { code = result.Error?.Code, description = result.Error?.Description });
        var newPoll = result.Value;
        var location = $"{Request.Scheme}://{Request.Host}/api/Polls/{newPoll.Id}";
        return Created(location, new { id = newPoll.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] CreatePollRequest request, int id, CancellationToken cancellationToken)
    {
        var result = await pollService.UpdateAsync(request, id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] string? reason, CancellationToken cancellationToken)
    {
        var deleted = await pollService.DeleteAsync(id, reason, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPut("{id}/toggle-publish")]
    public async Task<IActionResult> TogglePublish(int id, CancellationToken cancellationToken)
    {
        var result = await pollService.TogglePublishAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
