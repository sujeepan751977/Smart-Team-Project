using Recruitment_Project.DTOs.EmployerVerification;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IEmployerVerificationAdminService
    {
        Task<List<EmployerVerificationDto>> GetAllAsync();

        Task<EmployerVerificationDto?> GetByIdAsync(int id);

        Task RequestInformationAsync(int id, string feedback);

        Task VerifyAsync(int id);

        Task RejectAsync(int id, string feedback);
    }
}