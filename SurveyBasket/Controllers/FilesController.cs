using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Abstractions;
using SurveyBasket.Services;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Upload an image file
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromQuery] int? pollId, CancellationToken cancellationToken)
        {
            var result = await _fileService.UploadAsync(file, pollId, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get my uploaded files
        /// </summary>
        [HttpGet("my-files")]
        public async Task<IActionResult> GetMyFiles(CancellationToken cancellationToken)
        {
            var result = await _fileService.GetMyFilesAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get files uploaded by a specific user (any authenticated user can see)
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFiles(string userId, CancellationToken cancellationToken)
        {
            var result = await _fileService.GetUserFilesAsync(userId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get all uploaded files
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFiles(CancellationToken cancellationToken)
        {
            var result = await _fileService.GetAllFilesAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Delete a file (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = DefaultRoles.Admin)]
        public async Task<IActionResult> DeleteFile(int id, CancellationToken cancellationToken)
        {
            var result = await _fileService.DeleteFileAsync(id, cancellationToken);
            return result.IsSuccess ? NoContent() : NotFound(result.Error);
        }
    }
}
