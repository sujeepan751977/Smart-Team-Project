using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Repositories
{
    public class EmployerRepository : IEmployerRepository
    {
        private readonly AppDbContext _context;

        public EmployerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmployerProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.EmployerProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(EmployerProfile employerProfile)
        {
            await _context.EmployerProfiles.AddAsync(employerProfile);
        }

        public Task UpdateAsync(EmployerProfile employerProfile)
        {
            _context.EmployerProfiles.Update(employerProfile);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}