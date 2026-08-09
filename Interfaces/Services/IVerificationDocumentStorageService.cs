namespace Recruitment_Project.Interfaces.Services
{
    public interface IVerificationDocumentStorageService
    {
        Task<string> SaveAsync(
            Stream fileStream,
            string fileName,
            string contentType);

        Task DeleteAsync(string filePath);
    }
}