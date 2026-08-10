using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly AppDbContext _context;

        public ApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication?> GetByIdAsync(int id)
        {
            return await _context.JobApplications
                .Include(x => x.Vacancy)
                    .ThenInclude(x => x.EmployerProfile)
                .Include(x => x.JobSeekerProfile)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<JobApplication?> GetByVacancyAndJobSeekerAsync(
            int vacancyId,
            int jobSeekerProfileId)
        {
            return await _context.JobApplications
                .FirstOrDefaultAsync(x =>
                    x.VacancyId == vacancyId &&
                    x.JobSeekerProfileId == jobSeekerProfileId);
        }

        public async Task<List<JobApplication>> GetByJobSeekerAsync(
            int jobSeekerProfileId)
        {
            return await _context.JobApplications
                .Include(x => x.Vacancy)
                .Where(x => x.JobSeekerProfileId == jobSeekerProfileId)
                .OrderByDescending(x => x.AppliedAt)
                .ToListAsync();
        }

        public async Task<List<JobApplication>> GetByEmployerAsync(
            int employerProfileId)
        {
            return await _context.JobApplications
                .Include(x => x.Vacancy)
                .Include(x => x.JobSeekerProfile)
                .Where(x => x.Vacancy.EmployerProfileId == employerProfileId)
                .OrderByDescending(x => x.AppliedAt)
                .ToListAsync();
        }

        public async Task<List<JobApplication>> GetByVacancyAsync(
            int vacancyId)
        {
            return await _context.JobApplications
                .Include(x => x.JobSeekerProfile)
                .Include(x => x.Vacancy)
                .Where(x => x.VacancyId == vacancyId)
                .OrderByDescending(x => x.MatchScore)
                .ThenByDescending(x => x.AppliedAt)
                .ToListAsync();
        }

        public async Task AddAsync(JobApplication application)
        {
            await _context.JobApplications.AddAsync(application);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}