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
        private readonly INotificationService _notificationService;

        public JobReportService(
            IJobReportRepository jobReportRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _jobReportRepository = jobReportRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<JobReport> CreateReportAsync(
            int vacancyId,
            int userId,
            JobReportReason reason,
            string? description)
        {
            var exists = await _jobReportRepository
                .ExistsAsync(vacancyId, userId);

            if (exists)
            {
                throw new Exception(
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

            var users = await _userRepository.GetAllUsersAsync();

            var administrators = users
                .Where(x =>
                    x.Role == UserRole.Administrator &&
                    x.IsActive)
                .ToList();

            foreach (var administrator in administrators)
            {
                await _notificationService.CreateAsync(
                    new Notification
                    {
                        UserId = administrator.Id,
                        Type = NotificationType.JobReport,
                        Title = "New Job Report",
                        Message =
                            $"A new job report has been submitted for vacancy {vacancyId}."
                    });
            }

            return report;
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