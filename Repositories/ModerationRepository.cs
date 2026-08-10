using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Repositories
{
    public class ModerationRepository : IModerationRepository
    {
        private readonly AppDbContext _context;

        public ModerationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmployerProfile?> GetEmployerByIdAsync(
            int employerId)
        {
            return await _context.EmployerProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == employerId);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<List<Vacancy>> GetOpenVacanciesByEmployerIdAsync(
            int employerProfileId)
        {
            return await _context.Vacancies
                .Where(x =>
                    x.EmployerProfileId == employerProfileId &&
                    x.Status == VacancyStatus.Open)
                .ToListAsync();
        }

        public async Task<Vacancy?> GetVacancyByIdAsync(
            int vacancyId)
        {
            return await _context.Vacancies
                .Include(x => x.EmployerProfile)
                .FirstOrDefaultAsync(x => x.Id == vacancyId);
        }

        public async Task AddAuditLogAsync(
            ModerationAuditLog auditLog)
        {
            await _context.ModerationAuditLogs
                .AddAsync(auditLog);
        }

        public Task UpdateEmployerAsync(
            EmployerProfile employerProfile)
        {
            _context.EmployerProfiles.Update(employerProfile);

            return Task.CompletedTask;
        }

        public Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);

            return Task.CompletedTask;
        }

        public Task UpdateVacancyAsync(Vacancy vacancy)
        {
            _context.Vacancies.Update(vacancy);

            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}