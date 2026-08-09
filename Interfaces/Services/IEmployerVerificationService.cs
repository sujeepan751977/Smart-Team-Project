using Recruitment_Project.DTOs.Employers;
using Recruitment_Project.DTOs.EmployerVerification;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IEmployerVerificationService
    {
        Task<EmployerVerificationDto?> GetVerificationAsync(int userId);

        Task SubmitVerificationAsync(int userId);

        Task WithdrawVerificationAsync(int userId);

        Task ResubmitVerificationAsync(int userId);

        Task AddDocumentAsync(
            int userId,
            EmployerVerificationDocumentDto document);
        Task DeleteDocumentAsync(int userId, int documentId);

        Task UpdateCompanyInformationAsync(
    int userId,
    UpdateEmployerProfileDto dto);

        Task AddDocumentFileAsync(
    int userId,
    UploadEmployerVerificationDocumentDto request);
    }
}