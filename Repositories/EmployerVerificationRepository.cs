using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Repositories
{
    public class EmployerVerificationRepository : IEmployerVerificationRepository
    {
        private readonly AppDbContext _context;

        public EmployerVerificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmployerVerification?> GetByEmployerProfileIdAsync(int employerProfileId)
        {
            return await _context.EmployerVerifications
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.EmployerProfileId == employerProfileId);
        }

        public async Task AddAsync(EmployerVerification verification)
        {
            await _context.EmployerVerifications.AddAsync(verification);
        }

        public Task UpdateAsync(EmployerVerification verification)
        {
            _context.EmployerVerifications.Update(verification);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}