using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Files.Response;

namespace SurveyBasket.Services
{
    public interface IFileService
    {
        Task<Result<FileResponse>> UploadAsync(IFormFile file, int? pollId, CancellationToken cancellationToken);
        Task<IEnumerable<FileResponse>> GetMyFilesAsync(CancellationToken cancellationToken);
        Task<IEnumerable<FileResponse>> GetUserFilesAsync(string userId, CancellationToken cancellationToken);
        Task<IEnumerable<FileResponse>> GetAllFilesAsync(CancellationToken cancellationToken);
        Task<Result> DeleteFileAsync(int id, CancellationToken cancellationToken);
    }
}
