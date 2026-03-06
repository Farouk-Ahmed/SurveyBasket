using SurveyBasket.Abstractions;

namespace SurveyBasket.Errors
{
    public class FileErrors
    {
        public static readonly Error EmptyFile =
            new("File.Empty", "No file was provided or file is empty.");

        public static readonly Error FileTooLarge =
            new("File.TooLarge", "File size exceeds the maximum allowed size (5 MB).");

        public static readonly Error InvalidFileType =
            new("File.InvalidType", "Only image files are allowed (jpg, jpeg, png, gif, bmp, webp).");

        public static readonly Error FileNotFound =
            new("File.NotFound", "No file was found with the given ID.");

        public static readonly Error Unauthorized =
            new("File.Unauthorized", "You must be logged in to upload files.");
    }
}
