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
                .FirstOrDefaultAsync(x => x.JobSeekerProfile.UserId == userId);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}