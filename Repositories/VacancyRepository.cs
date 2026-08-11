using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Repositories
{
    public class VacancyRepository : IVacancyRepository
    {
        private readonly AppDbContext _context;

        public VacancyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vacancy>> GetByEmployerProfileIdAsync(
            int employerProfileId)
        {
            return await _context.Vacancies
                .Include(x => x.RequiredSkills)
                    .ThenInclude(x => x.Skill)
                .Where(x => x.EmployerProfileId == employerProfileId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Vacancy?> GetByIdAsync(int vacancyId)
        {
            return await _context.Vacancies
                .Include(x => x.EmployerProfile)
                .Include(x => x.RequiredSkills)
                    .ThenInclude(x => x.Skill)
                .FirstOrDefaultAsync(x => x.Id == vacancyId);
        }

        public async Task<List<Vacancy>> GetByStatusAsync(
            VacancyStatus status)
        {
            return await _context.Vacancies
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Vacancy vacancy)
        {
            await _context.Vacancies.AddAsync(vacancy);
        }

        public Task UpdateAsync(Vacancy vacancy)
        {
            _context.Vacancies.Update(vacancy);
            return Task.CompletedTask;
        }

        public async Task AddVacancySkillAsync(VacancySkill vacancySkill)
        {
            await _context.VacancySkills.AddAsync(vacancySkill);
        }

        public Task RemoveVacancySkillAsync(VacancySkill vacancySkill)
        {
            _context.VacancySkills.Remove(vacancySkill);
            return Task.CompletedTask;
        }

        public async Task<Skill> GetOrCreateSkillAsync(string skillName)
        {
            var normalized = skillName.Trim();

            var existing = await _context.Skills
                .FirstOrDefaultAsync(x =>
                    x.Name.ToLower() == normalized.ToLower());

            if (existing != null)
                return existing;

            var skill = new Skill
            {
                Name = normalized,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();

            return skill;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
