using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class JobReportService : IJobReportService
    {
        private readonly IJobReportRepository _jobReportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly INotificationService _notificationService;

        public JobReportService(
            IJobReportRepository jobReportRepository,
            IUserRepository userRepository,
            IVacancyRepository vacancyRepository,
            INotificationService notificationService)
        {
            _jobReportRepository = jobReportRepository;
            _userRepository = userRepository;
            _vacancyRepository = vacancyRepository;
            _notificationService = notificationService;
        }

        public async Task<JobReport> CreateReportAsync(
            int vacancyId,
            int userId,
            JobReportReason reason,
            string? description)
        {
            var vacancyExists =
                await _vacancyRepository.GetByIdAsync(vacancyId) != null;

            if (!vacancyExists)
            {
                throw new KeyNotFoundException("Vacancy not found.");
            }

            if (!Enum.IsDefined(typeof(JobReportReason), reason))
            {
                throw new InvalidOperationException("Invalid report reason.");
            }

            var exists = await _jobReportRepository
                .ExistsAsync(vacancyId, userId);

            if (exists)
            {
                throw new InvalidOperationException(
                    "You have already reported this job.");
            }

            var report = new JobReport
            {
                VacancyId = vacancyId,
                ReportedByUserId = userId,
                Reason = reason,
                Description = description,
                Status = JobReportStatus.Pending,
                ReportedAt = DateTime.UtcNow
            };

            await _jobReportRepository.AddAsync(report);
            await _jobReportRepository.SaveChangesAsync();

            var administratorIds = (await _userRepository.GetAllUsersAsync())
                .Where(x =>
                    x.Role == UserRole.Administrator &&
                    x.IsActive)
                .Select(x => x.Id)
                .ToList();

            foreach (var administratorId in administratorIds)
            {
                await _notificationService.CreateAsync(
                    new Notification
                    {
                        UserId = administratorId,
                        Type = NotificationType.JobReport,
                        Title = "New Job Report",
                        Message =
                            $"A new job report has been submitted for vacancy {vacancyId}."
                    });
            }

            // Return a detached copy without navigation graphs for safe JSON output.
            return new JobReport
            {
                Id = report.Id,
                VacancyId = report.VacancyId,
                ReportedByUserId = report.ReportedByUserId,
                Reason = report.Reason,
                Description = report.Description,
                Status = report.Status,
                ReportedAt = report.ReportedAt,
                ReviewedAt = report.ReviewedAt
            };
        }

        public async Task<IEnumerable<JobReport>> GetMyReportsAsync(
            int userId)
        {
            return await _jobReportRepository
                .GetByUserIdAsync(userId);
        }

        public async Task<JobReport?> GetReportByIdAsync(
            int id,
            int userId)
        {
            var report = await _jobReportRepository
                .GetByIdAsync(id);

            if (report == null)
                return null;

            if (report.ReportedByUserId != userId)
                return null;

            return report;
        }
    }
}
