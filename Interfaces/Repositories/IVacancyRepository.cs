using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IVacancyRepository
    {
        Task<List<Vacancy>> GetByEmployerProfileIdAsync(int employerProfileId);

        Task<Vacancy?> GetByIdAsync(int vacancyId);

        Task<List<Vacancy>> GetByStatusAsync(VacancyStatus status);

        Task AddAsync(Vacancy vacancy);

        Task UpdateAsync(Vacancy vacancy);

        Task SaveChangesAsync();
    }
}