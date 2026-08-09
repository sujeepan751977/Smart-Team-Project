using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class VacancyApprovalService : IVacancyApprovalService
    {
        private readonly IVacancyRepository _vacancyRepository;

        public VacancyApprovalService(
            IVacancyRepository vacancyRepository)
        {
            _vacancyRepository = vacancyRepository;
        }

        public async Task<List<Vacancy>> GetPendingVacanciesAsync()
        {
            return await _vacancyRepository
                .GetByStatusAsync(VacancyStatus.PendingApproval);
        }

        public async Task ApproveVacancyAsync(int vacancyId)
        {
            var vacancy = await _vacancyRepository
                .GetByIdAsync(vacancyId);

            if (vacancy == null)
                throw new Exception("Vacancy not found");

            if (vacancy.Status != VacancyStatus.PendingApproval)
                throw new Exception(
                    "Only pending vacancies can be approved");

            vacancy.Status = VacancyStatus.Open;
            vacancy.UpdatedAt = DateTime.UtcNow;

            await _vacancyRepository.UpdateAsync(vacancy);
            await _vacancyRepository.SaveChangesAsync();
        }

        public async Task RejectVacancyAsync(
            int vacancyId,
            string reason)
        {
            var vacancy = await _vacancyRepository
                .GetByIdAsync(vacancyId);

            if (vacancy == null)
                throw new Exception("Vacancy not found");

            if (vacancy.Status != VacancyStatus.PendingApproval)
                throw new Exception(
                    "Only pending vacancies can be rejected");

            vacancy.Status = VacancyStatus.Rejected;
            vacancy.AdministratorRejectionReason = reason;
            vacancy.UpdatedAt = DateTime.UtcNow;

            await _vacancyRepository.UpdateAsync(vacancy);
            await _vacancyRepository.SaveChangesAsync();
        }
    }
}