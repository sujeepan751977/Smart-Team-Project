using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class JobReportAdminService : IJobReportAdminService
    {
        private readonly IJobReportRepository _jobReportRepository;

        public JobReportAdminService(
            IJobReportRepository jobReportRepository)
        {
            _jobReportRepository = jobReportRepository;
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
                return;

            report.Status = JobReportStatus.UnderReview;
            report.ReviewedAt = DateTime.UtcNow;

            await _jobReportRepository.UpdateAsync(report);
            await _jobReportRepository.SaveChangesAsync();
        }

        public async Task ResolveReportAsync(int reportId)
        {
            var report = await _jobReportRepository
                .GetByIdAsync(reportId);

            if (report == null)
                return;

            report.Status = JobReportStatus.ActionTaken;
            report.ReviewedAt = DateTime.UtcNow;

            await _jobReportRepository.UpdateAsync(report);
            await _jobReportRepository.SaveChangesAsync();
        }

        public async Task DismissReportAsync(int reportId)
        {
            var report = await _jobReportRepository
                .GetByIdAsync(reportId);

            if (report == null)
                return;

            report.Status = JobReportStatus.Rejected;
            report.ReviewedAt = DateTime.UtcNow;

            await _jobReportRepository.UpdateAsync(report);
            await _jobReportRepository.SaveChangesAsync();
        }
    }
}