using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class ModerationService : IModerationService
    {
        private readonly IModerationRepository _moderationRepository;
        private readonly INotificationService _notificationService;

        public ModerationService(
            IModerationRepository moderationRepository,
            INotificationService notificationService)
        {
            _moderationRepository = moderationRepository;
            _notificationService = notificationService;
        }

        public async Task WarnEmployerAsync(
            int adminUserId,
            int employerId,
            string decisionNote)
        {
            var employer = await _moderationRepository
                .GetEmployerByIdAsync(employerId);

            if (employer == null)
                throw new KeyNotFoundException("Employer not found.");

            var previousValue = employer.AccountStatus.ToString();
            var newValue = EmployerAccountStatus.Warned.ToString();

            employer.AccountStatus = EmployerAccountStatus.Warned;
            employer.UpdatedAt = DateTime.UtcNow;

            await _moderationRepository
                .UpdateEmployerAsync(employer);

            await CreateAuditLogAsync(
                adminUserId,
                ModerationActionType.WarnEmployer,
                "EmployerProfile",
                employerId,
                decisionNote,
                previousValue,
                newValue);

            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = employer.UserId,
                    Type = NotificationType.Moderation,
                    Title = "Employer Account Warning",
                    Message =
                        $"Your employer account has been warned. Reason: {decisionNote}"
                });

            await _moderationRepository.SaveChangesAsync();
        }

        public async Task SuspendEmployerAsync(
            int adminUserId,
            int employerId,
            string decisionNote)
        {
            var employer = await _moderationRepository
                .GetEmployerByIdAsync(employerId);

            if (employer == null)
                throw new KeyNotFoundException("Employer not found.");

            var previousValue = employer.AccountStatus.ToString();
            var newValue = EmployerAccountStatus.Suspended.ToString();

            employer.AccountStatus = EmployerAccountStatus.Suspended;
            employer.UpdatedAt = DateTime.UtcNow;

            await _moderationRepository
                .UpdateEmployerAsync(employer);

            var openVacancies =
                await _moderationRepository
                    .GetOpenVacanciesByEmployerIdAsync(employerId);

            foreach (var vacancy in openVacancies)
            {
                vacancy.Status = VacancyStatus.Suspended;
                vacancy.UpdatedAt = DateTime.UtcNow;

                await _moderationRepository
                    .UpdateVacancyAsync(vacancy);
            }

            await CreateAuditLogAsync(
                adminUserId,
                ModerationActionType.SuspendEmployer,
                "EmployerProfile",
                employerId,
                decisionNote,
                previousValue,
                newValue);

            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = employer.UserId,
                    Type = NotificationType.Moderation,
                    Title = "Employer Account Suspended",
                    Message =
                        $"Your employer account has been suspended. Reason: {decisionNote}"
                });

            await _moderationRepository.SaveChangesAsync();
        }

        public async Task DisableEmployerAsync(
            int adminUserId,
            int employerId,
            string decisionNote)
        {
            var employer = await _moderationRepository
                .GetEmployerByIdAsync(employerId);

            if (employer == null)
                throw new KeyNotFoundException("Employer not found.");

            var previousValue = employer.AccountStatus.ToString();
            var newValue = EmployerAccountStatus.Disabled.ToString();

            employer.AccountStatus = EmployerAccountStatus.Disabled;
            employer.UpdatedAt = DateTime.UtcNow;

            await _moderationRepository
                .UpdateEmployerAsync(employer);

            var user = await _moderationRepository
                .GetUserByIdAsync(employer.UserId);

            if (user == null)
                throw new KeyNotFoundException(
                    "Employer user not found.");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _moderationRepository
                .UpdateUserAsync(user);

            await CreateAuditLogAsync(
                adminUserId,
                ModerationActionType.DisableEmployer,
                "EmployerProfile",
                employerId,
                decisionNote,
                previousValue,
                newValue);

            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = employer.UserId,
                    Type = NotificationType.Moderation,
                    Title = "Employer Account Disabled",
                    Message =
                        $"Your employer account has been disabled. Reason: {decisionNote}"
                });

            await _moderationRepository.SaveChangesAsync();
        }

        public async Task CloseVacancyAsync(
            int adminUserId,
            int vacancyId,
            string decisionNote)
        {
            var vacancy = await _moderationRepository
                .GetVacancyByIdAsync(vacancyId);

            if (vacancy == null)
                throw new KeyNotFoundException(
                    "Vacancy not found.");

            var previousValue = vacancy.Status.ToString();
            var newValue = VacancyStatus.Closed.ToString();

            vacancy.Status = VacancyStatus.Closed;
            vacancy.UpdatedAt = DateTime.UtcNow;

            await _moderationRepository
                .UpdateVacancyAsync(vacancy);

            await CreateAuditLogAsync(
                adminUserId,
                ModerationActionType.CloseVacancy,
                "Vacancy",
                vacancyId,
                decisionNote,
                previousValue,
                newValue);

            // Notify Employer
            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = vacancy.EmployerProfile.UserId,
                    Type = NotificationType.Moderation,
                    Title = "Vacancy Closed",
                    Message =
                        $"Your vacancy \"{vacancy.Title}\" has been closed by an administrator. " +
                        $"Reason: {decisionNote}"
                });

            await _moderationRepository.SaveChangesAsync();
        }

        private async Task CreateAuditLogAsync(
            int adminUserId,
            ModerationActionType actionType,
            string targetEntity,
            int targetEntityId,
            string decisionNote,
            string? previousValue,
            string? newValue)
        {
            var auditLog = new ModerationAuditLog
            {
                AdminUserId = adminUserId,
                ActionType = actionType,
                TargetEntity = targetEntity,
                TargetEntityId = targetEntityId,
                Reason = decisionNote,
                PreviousValue = previousValue,
                NewValue = newValue,
                ActionDate = DateTime.UtcNow
            };

            await _moderationRepository
                .AddAuditLogAsync(auditLog);
        }
    }
}