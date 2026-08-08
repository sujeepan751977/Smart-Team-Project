using Recruitment_Project.DTOs.EmployerVerification;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IEmployerVerificationService
    {
        Task<EmployerVerificationDto?> GetVerificationAsync(int userId);

        Task SubmitVerificationAsync(int userId);

        Task AddDocumentAsync(
            int userId,
            EmployerVerificationDocumentDto document);
    }
}