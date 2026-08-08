using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;

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
            var query = _context.Vacancies
                .Where(x => x.Status.ToString() == "Open")
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
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }



        public async Task<Vacancy?> GetJobDetailsAsync(int id)
        {
            return await _context.Vacancies
                .Include(x => x.EmployerProfile)
                .Include(x => x.RequiredSkills)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}