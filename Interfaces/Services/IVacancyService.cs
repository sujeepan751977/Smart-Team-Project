using Recruitment_Project.DTOs.Jobs;
using Recruitment_Project.DTOs.Vacancies;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IVacancyService
    {
        Task<List<EmployerVacancyDto>> GetMyVacanciesAsync(int userId);

        Task<EmployerVacancyDto?> GetMyVacancyByIdAsync(int userId,int vacancyId);

        Task<int> CreateVacancyAsync(int userId, CreateVacancyDto dto);

        Task UpdateVacancyAsync(int userId, int vacancyId, UpdateVacancyDto dto);

        Task SubmitVacancyAsync(int userId, int vacancyId);

        Task CloseVacancyAsync(int userId, int vacancyId);
    }
}