using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Repositories
{
    public class JobSearchRepository : IJobSearchRepository
    {
        private readonly AppDbContext _context;

        public JobSearchRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<Vacancy>> SearchJobsAsync(
            string? search,
            string? company,
            string? skill,
            string? location,
            int? experience,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            var now = DateTime.UtcNow;

            var query = _context.Vacancies
                .Where(x =>
                    x.Status == VacancyStatus.Open &&
                    x.ExpiryDate >= now &&
                    x.EmployerProfile.AccountStatus != EmployerAccountStatus.Disabled &&
                    x.EmployerProfile.AccountStatus != EmployerAccountStatus.Suspended)
                .AsQueryable();


            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.Title.Contains(search));
            }


            if (!string.IsNullOrEmpty(company))
            {
                query = query.Where(x =>
                    x.EmployerProfile.CompanyName.Contains(company));
            }


            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(x =>
                    x.WorkLocation.Contains(location));
            }


            if (!string.IsNullOrEmpty(skill))
            {
                query = query.Where(x =>
                    x.RequiredSkills.Any(s =>
                        s.Skill.Name.Contains(skill)));
            }


            if (experience.HasValue)
            {
                query = query.Where(x =>
                    x.ExperienceLevel.Contains(experience.Value.ToString()));
            }


            return await query
                .Include(x => x.EmployerProfile)
                .Include(x => x.RequiredSkills)
                    .ThenInclude(x => x.Skill)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }



        public async Task<Vacancy?> GetJobDetailsAsync(int id)
        {
            var now = DateTime.UtcNow;

            return await _context.Vacancies
                .Include(x => x.EmployerProfile)
                .Include(x => x.RequiredSkills)
                    .ThenInclude(x => x.Skill)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.Status == VacancyStatus.Open &&
                    x.ExpiryDate >= now &&
                    x.EmployerProfile.AccountStatus != EmployerAccountStatus.Disabled &&
                    x.EmployerProfile.AccountStatus != EmployerAccountStatus.Suspended);
        }
    }
}
