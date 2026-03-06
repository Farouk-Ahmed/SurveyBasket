namespace SurveyBasket.Contract.Files.Response
{
    public record FileResponse(
        int Id,
        string FileName,
        string FileUrl,
        string ContentType,
        long FileSize,
        string UploadedByName,
        string UploadedById,
        DateTime UploadedOn,
        int? PollId
    );
}
