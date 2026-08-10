using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IModerationRepository
    {
        Task<EmployerProfile?> GetEmployerByIdAsync(int employerId);

        Task<User?> GetUserByIdAsync(int userId);

        Task<List<Vacancy>> GetOpenVacanciesByEmployerIdAsync(
            int employerProfileId);

        Task<Vacancy?> GetVacancyByIdAsync(int vacancyId);

        Task AddAuditLogAsync(ModerationAuditLog auditLog);

        Task UpdateEmployerAsync(EmployerProfile employerProfile);

        Task UpdateUserAsync(User user);

        Task UpdateVacancyAsync(Vacancy vacancy);

        Task SaveChangesAsync();
    }
}