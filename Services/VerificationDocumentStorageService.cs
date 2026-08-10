using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Middleware;

namespace Recruitment_Project.Services
{
    public class VerificationDocumentStorageService
        : IVerificationDocumentStorageService
    {
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] AllowedExtensions =
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png"
        };

        private const long MaxFileSize = 5 * 1024 * 1024;

        public VerificationDocumentStorageService(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveAsync(
            Stream fileStream,
            string fileName,
            string contentType)
        {
            var extension =
                Path.GetExtension(fileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                throw new ValidationException(
    "Only PDF, JPG, JPEG and PNG files are allowed.");
            }

            if (fileStream.Length > MaxFileSize)
            {
                throw new ValidationException(
    "File size cannot exceed 5 MB.");
            }

            var folderPath = Path.Combine(
                _environment.ContentRootPath,
                "Storage",
                "EmployerVerificationDocuments");

            Directory.CreateDirectory(folderPath);

            var safeFileName =
                $"{Guid.NewGuid()}{extension}";

            var fullPath =
                Path.Combine(folderPath, safeFileName);

            await using var fileStreamOutput =
                new FileStream(
                    fullPath,
                    FileMode.Create);

            await fileStream.CopyToAsync(fileStreamOutput);

            return Path.Combine(
                "Storage",
                "EmployerVerificationDocuments",
                safeFileName)
                .Replace("\\", "/");
        }

        public Task DeleteAsync(string filePath)
        {
            var fullPath = Path.Combine(
                _environment.ContentRootPath,
                filePath.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }
}