using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IVacancyApprovalService
    {
        Task<List<Vacancy>> GetPendingVacanciesAsync();

        Task ApproveVacancyAsync(int vacancyId);

        Task RejectVacancyAsync(int vacancyId, string reason);
    }
}