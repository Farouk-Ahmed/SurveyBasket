using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Abstractions;

namespace SurveyBasket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FilesController(IFileService fileService) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] int? pollId, CancellationToken cancellationToken)
    {
        var result = await fileService.UploadAsync(file, pollId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("my-files")]
    public async Task<IActionResult> GetMyFiles(CancellationToken cancellationToken)
    {
        var result = await fileService.GetMyFilesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserFiles(string userId, CancellationToken cancellationToken)
    {
        var result = await fileService.GetUserFilesAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllFiles(CancellationToken cancellationToken)
    {
        var result = await fileService.GetAllFilesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = DefaultRoles.Admin)]
    public async Task<IActionResult> DeleteFile(int id, CancellationToken cancellationToken)
    {
        var result = await fileService.DeleteFileAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
