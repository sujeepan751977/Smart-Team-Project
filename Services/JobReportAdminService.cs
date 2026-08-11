using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class JobReportAdminService : IJobReportAdminService
    {
        private readonly IJobReportRepository _jobReportRepository;
        private readonly INotificationService _notificationService;

        public JobReportAdminService(
            IJobReportRepository jobReportRepository,
            INotificationService notificationService)
        {
            _jobReportRepository = jobReportRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<JobReport>> GetAllReportsAsync()
        {
            return await _jobReportRepository.GetAllAsync();
        }

        public async Task<IEnumerable<JobReport>> GetReportsByStatusAsync(
            JobReportStatus status)
        {
            return await _jobReportRepository.GetByStatusAsync(status);
        }

        public async Task<JobReport?> GetReportDetailsAsync(
            int reportId)
        {
            return await _jobReportRepository.GetByIdAsync(reportId);
        }

        public async Task StartReviewAsync(int reportId)
        {
            var report = await _jobReportRepository
                .GetByIdAsync(reportId);

            if (report == null)
                throw new KeyNotFoundException("Job report not found.");

            if (report.Status != JobReportStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending reports can be moved to under review.");

            report.Status = JobReportStatus.UnderReview;
            report.ReviewedAt = DateTime.UtcNow;

            await _jobReportRepository.UpdateAsync(report);
            await _jobReportRepository.SaveChangesAsync();

            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = report.ReportedByUserId,
                    Type = NotificationType.JobReport,
                    Title = "Job Report Under Review",
                    Message =
                        "Your job report has been received and is now under review."
                });
        }

        public async Task ResolveReportAsync(int reportId)
        {
            var report = await _jobReportRepository
                .GetByIdAsync(reportId);

            if (report == null)
                throw new KeyNotFoundException("Job report not found.");

            if (report.Status is JobReportStatus.ActionTaken
                or JobReportStatus.Rejected)
            {
                throw new InvalidOperationException(
                    "This report has already been closed.");
            }

            report.Status = JobReportStatus.ActionTaken;
            report.ReviewedAt = DateTime.UtcNow;

            await _jobReportRepository.UpdateAsync(report);
            await _jobReportRepository.SaveChangesAsync();

            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = report.ReportedByUserId,
                    Type = NotificationType.JobReport,
                    Title = "Job Report Resolved",
                    Message =
                        "Your job report has been reviewed and appropriate action has been taken."
                });
        }

        public async Task DismissReportAsync(int reportId)
        {
            var report = await _jobReportRepository
                .GetByIdAsync(reportId);

            if (report == null)
                throw new KeyNotFoundException("Job report not found.");

            if (report.Status is JobReportStatus.ActionTaken
                or JobReportStatus.Rejected)
            {
                throw new InvalidOperationException(
                    "This report has already been closed.");
            }

            report.Status = JobReportStatus.Rejected;
            report.ReviewedAt = DateTime.UtcNow;

            await _jobReportRepository.UpdateAsync(report);
            await _jobReportRepository.SaveChangesAsync();

            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = report.ReportedByUserId,
                    Type = NotificationType.JobReport,
                    Title = "Job Report Dismissed",
                    Message =
                        "Your job report has been reviewed and dismissed."
                });
        }
    }
}
