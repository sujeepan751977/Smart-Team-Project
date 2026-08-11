using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class VacancyApprovalService : IVacancyApprovalService
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly INotificationService _notificationService;

        public VacancyApprovalService(
            IVacancyRepository vacancyRepository,
            INotificationService notificationService)
        {
            _vacancyRepository = vacancyRepository;
            _notificationService = notificationService;
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
                throw new KeyNotFoundException("Vacancy not found");

            if (vacancy.Status != VacancyStatus.PendingApproval)
                throw new InvalidOperationException(
                    "Only pending vacancies can be approved");

            if (vacancy.EmployerProfile.AccountStatus ==
                    EmployerAccountStatus.Suspended ||
                vacancy.EmployerProfile.AccountStatus ==
                    EmployerAccountStatus.Disabled)
            {
                throw new InvalidOperationException(
                    "Cannot approve vacancy for a suspended or disabled employer.");
            }

            vacancy.Status = VacancyStatus.Open;
            vacancy.UpdatedAt = DateTime.UtcNow;

            await _vacancyRepository.UpdateAsync(vacancy);
            await _vacancyRepository.SaveChangesAsync();

            // Notify Employer
            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = vacancy.EmployerProfile.UserId,
                    Type = NotificationType.VacancyApproval,
                    Title = "Vacancy Approved",
                    Message =
                        $"Your vacancy \"{vacancy.Title}\" has been approved and is now open."
                });
        }

        public async Task RejectVacancyAsync(
            int vacancyId,
            string reason)
        {
            var vacancy = await _vacancyRepository
                .GetByIdAsync(vacancyId);

            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found");

            if (vacancy.Status != VacancyStatus.PendingApproval)
                throw new InvalidOperationException(
                    "Only pending vacancies can be rejected");

            vacancy.Status = VacancyStatus.Rejected;
            vacancy.AdministratorRejectionReason = reason;
            vacancy.UpdatedAt = DateTime.UtcNow;

            await _vacancyRepository.UpdateAsync(vacancy);
            await _vacancyRepository.SaveChangesAsync();

            // Notify Employer
            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = vacancy.EmployerProfile.UserId,
                    Type = NotificationType.VacancyApproval,
                    Title = "Vacancy Rejected",
                    Message =
                        $"Your vacancy \"{vacancy.Title}\" has been rejected. " +
                        $"Reason: {reason}"
                });
        }
    }
}