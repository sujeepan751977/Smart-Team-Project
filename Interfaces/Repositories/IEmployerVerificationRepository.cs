using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IEmployerVerificationRepository
    {
        Task<EmployerVerification?> GetByEmployerProfileIdAsync(int employerProfileId);

        Task<List<EmployerVerification>> GetAllAsync();

        Task<EmployerVerification?> GetByIdAsync(int id);

        Task AddAsync(EmployerVerification verification);

        Task DeleteDocumentAsync(int documentId);

        Task UpdateAsync(EmployerVerification verification);

        Task SaveChangesAsync();
    }
}