using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IEmployerVerificationRepository
    {
        Task<EmployerVerification?> GetByEmployerProfileIdAsync(int employerProfileId);

        Task AddAsync(EmployerVerification verification);

        Task UpdateAsync(EmployerVerification verification);

        Task SaveChangesAsync();
    }
}