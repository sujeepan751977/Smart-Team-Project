using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Repositories
{
    public class CvRepository : ICvRepository
    {
        private readonly AppDbContext _context;

        public CvRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(CvDocument cvDocument)
        {
            await _context.CvDocuments.AddAsync(cvDocument);
        }


        public async Task<CvDocument?> GetByUserIdAsync(int userId)
        {
            return await _context.CvDocuments
                .Include(x => x.JobSeekerProfile)
                .Where(x => x.JobSeekerProfile.UserId == userId)
                .OrderByDescending(x => x.UploadedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<CvDocument?> GetLatestByJobSeekerProfileIdAsync(
            int jobSeekerProfileId)
        {
            return await _context.CvDocuments
                .Where(x => x.JobSeekerProfileId == jobSeekerProfileId)
                .OrderByDescending(x => x.UploadedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CvDocument>> GetAllByUserIdAsync(int userId)
        {
            return await _context.CvDocuments
                .Include(x => x.JobSeekerProfile)
                .Where(x => x.JobSeekerProfile.UserId == userId)
                .ToListAsync();
        }


        public Task DeleteAsync(CvDocument cvDocument)
        {
            _context.CvDocuments.Remove(cvDocument);
            return Task.CompletedTask;
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
