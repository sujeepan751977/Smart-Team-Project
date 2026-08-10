using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Repositories
{
    public class ContactRequestRepository : IContactRequestRepository
    {
        private readonly AppDbContext _context;

        public ContactRequestRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContactRequest?> GetByIdAsync(int id)
        {
            return await _context.ContactRequests
                .Include(x => x.JobApplication)
                    .ThenInclude(x => x.Vacancy)
                .Include(x => x.EmployerProfile)
                .Include(x => x.JobSeekerProfile)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ContactRequest?> GetPendingRequestAsync(
            int jobApplicationId)
        {
            return await _context.ContactRequests
                .FirstOrDefaultAsync(x =>
                    x.JobApplicationId == jobApplicationId &&
                    x.Status == Models.Enums.ContactRequestStatus.Pending);
        }

        public async Task<List<ContactRequest>> GetByEmployerAsync(
            int employerProfileId)
        {
            return await _context.ContactRequests
                .Where(x => x.EmployerProfileId == employerProfileId)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<ContactRequest>> GetByJobSeekerAsync(
            int jobSeekerProfileId)
        {
            return await _context.ContactRequests
                .Where(x => x.JobSeekerProfileId == jobSeekerProfileId)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync();
        }

        public async Task AddAsync(
            ContactRequest contactRequest)
        {
            await _context.ContactRequests
                .AddAsync(contactRequest);
        }

        public Task UpdateAsync(
            ContactRequest contactRequest)
        {
            _context.ContactRequests.Update(contactRequest);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}