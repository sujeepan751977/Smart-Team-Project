using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.DTOs.Employers;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

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

        public async Task<EmployerDashboardDto> GetDashboardAsync(int employerProfileId)
        {
            var vacancies = await _context.Vacancies
                .Where(x => x.EmployerProfileId == employerProfileId)
                .ToListAsync();

            return new EmployerDashboardDto
            {
                TotalVacancies = vacancies.Count,
                OpenVacancies = vacancies.Count(x => x.Status == VacancyStatus.Open),
                PendingVacancies = vacancies.Count(x => x.Status == VacancyStatus.PendingApproval),
                ClosedVacancies = vacancies.Count(x => x.Status == VacancyStatus.Closed)
            };
        }
    }
}