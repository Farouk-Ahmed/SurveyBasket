using Microsoft.AspNetCore.Http;
using SurveyBasket.Application.DTOs.Files;
using SurveyBasket.Domain.Abstractions;

namespace SurveyBasket.Application.Interfaces;

public interface IFileService
{
    Task<Result<FileResponse>> UploadAsync(IFormFile file, int? pollId, CancellationToken cancellationToken);
    Task<IEnumerable<FileResponse>> GetMyFilesAsync(CancellationToken cancellationToken);
    Task<IEnumerable<FileResponse>> GetUserFilesAsync(string userId, CancellationToken cancellationToken);
    Task<IEnumerable<FileResponse>> GetAllFilesAsync(CancellationToken cancellationToken);
    Task<Result> DeleteFileAsync(int id, CancellationToken cancellationToken);
}
