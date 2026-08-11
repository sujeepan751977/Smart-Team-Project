using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Repositories
{
    public class JobSeekerRepository : IJobSeekerRepository
    {
        private readonly AppDbContext _context;

        public JobSeekerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobSeekerProfile?> GetProfileByUserIdAsync(int userId)
        {
            return await _context.JobSeekerProfiles
                .Include(x => x.Skills)
                .ThenInclude(x => x.Skill)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }


        public async Task UpdateProfileAsync(JobSeekerProfile profile)
        {
            _context.JobSeekerProfiles.Update(profile);
            await Task.CompletedTask;
        }


        public async Task AddSkillAsync(JobSeekerSkill jobSeekerSkill)
        {
            await _context.JobSeekerSkills.AddAsync(jobSeekerSkill);
        }


        public async Task RemoveSkillAsync(JobSeekerSkill jobSeekerSkill)
        {
            _context.JobSeekerSkills.Remove(jobSeekerSkill);
            await Task.CompletedTask;
        }


        public async Task<bool> SkillExistsAsync(int jobSeekerProfileId, string skillName)
        {
            return await _context.JobSeekerSkills
                .Include(x => x.Skill)
                .AnyAsync(x =>
                    x.JobSeekerProfileId == jobSeekerProfileId &&
                    x.Skill.Name.ToLower() == skillName.Trim().ToLower());
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

        public async Task AddProfileAsync(JobSeekerProfile profile)
        {
            await _context.JobSeekerProfiles.AddAsync(profile);
        }

       
    }
}
