using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IJobSeekerRepository
    {
        Task<JobSeekerProfile?> GetProfileByUserIdAsync(int userId);

        Task AddProfileAsync(JobSeekerProfile profile);

        Task UpdateProfileAsync(JobSeekerProfile profile);

        Task AddSkillAsync(JobSeekerSkill jobSeekerSkill);

        Task RemoveSkillAsync(JobSeekerSkill jobSeekerSkill);

        Task<bool> SkillExistsAsync(int jobSeekerProfileId, string skillName);

        Task<Skill> GetOrCreateSkillAsync(string skillName);

        Task SaveChangesAsync();
    }
}
