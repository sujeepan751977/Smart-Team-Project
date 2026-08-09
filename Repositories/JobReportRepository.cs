using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Repositories
{
    public class JobReportRepository : IJobReportRepository
    {
        private readonly AppDbContext _context;

        public JobReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobReport>> GetByUserIdAsync(int userId)
        {
            return await _context.JobReports
                .Where(x => x.ReportedByUserId == userId)
                .OrderByDescending(x => x.ReportedAt)
                .ToListAsync();
        }

        public async Task<JobReport?> GetByIdAsync(int id)
        {
            return await _context.JobReports
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ExistsAsync(int vacancyId, int userId)
        {
            return await _context.JobReports
                .AnyAsync(x =>
                    x.VacancyId == vacancyId &&
                    x.ReportedByUserId == userId);
        }

        public async Task AddAsync(JobReport jobReport)
        {
            await _context.JobReports.AddAsync(jobReport);
        }

        public Task UpdateAsync(JobReport jobReport)
        {
            _context.JobReports.Update(jobReport);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<JobReport>> GetAllAsync()
        {
            return await _context.JobReports
                .OrderByDescending(x => x.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobReport>> GetByStatusAsync(
            JobReportStatus status)
        {
            return await _context.JobReports
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.ReportedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}