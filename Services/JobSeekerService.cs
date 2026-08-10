using Recruitment_Project.DTOs.JobSeekers;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Services
{
    public class JobSeekerService : IJobSeekerService
    {
        private readonly IJobSeekerRepository _repository;

        public JobSeekerService(
            IJobSeekerRepository repository)
        {
            _repository = repository;
        }


        public async Task<JobSeekerProfileDto?> GetProfileAsync(int userId)
        {
            var profile = await _repository
                .GetProfileByUserIdAsync(userId);

            if (profile == null)
                return null;


            return new JobSeekerProfileDto
            {
                Id = profile.Id,

                ProfessionalTitle = profile.ProfessionalTitle,

                Location = profile.Location,

                Experience = profile.ExperienceInYears,

                Education = profile.Education,

                About = profile.About,

                Skills = profile.Skills
                    .Select(x => x.Skill.Name)
                    .ToList(),

                ProfileCompletion =
                    profile.ProfileCompletionPercentage
            };
        }



        public async Task<bool> UpdateProfileAsync(
            int userId,
            UpdateJobSeekerProfileDto dto)
        {
            var profile = await _repository
                .GetProfileByUserIdAsync(userId);


            if (profile == null)
                return false;


            profile.ProfessionalTitle = dto.ProfessionalTitle;

            profile.Location = dto.Location;

            profile.ExperienceInYears = dto.Experience;

            profile.Education = dto.Education;

            profile.About = dto.About;


            var existingSkills =
                profile.Skills.ToList();

            foreach (var existingSkill in existingSkills)
            {
                await _repository.RemoveSkillAsync(existingSkill);
            }

            var skillNames = (dto.Skills ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var skillName in skillNames)
            {
                var skill =
                    await _repository.GetOrCreateSkillAsync(skillName);

                await _repository.AddSkillAsync(new JobSeekerSkill
                {
                    JobSeekerProfileId = profile.Id,
                    SkillId = skill.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }


            profile.ProfileCompletionPercentage =
                CalculateCompletion(profile, skillNames.Count);



            await _repository
                .UpdateProfileAsync(profile);


            await _repository
                .SaveChangesAsync();


            return true;
        }



        public async Task<JobSeekerDashboardDto?> GetDashboardAsync(int userId)
        {
            var profile = await _repository
                .GetProfileByUserIdAsync(userId);


            if (profile == null)
                return null;


            return new JobSeekerDashboardDto
            {
                ProfessionalTitle = profile.ProfessionalTitle,

                ProfileCompletion =
                    profile.ProfileCompletionPercentage,

                TotalSkills =
                    profile.Skills.Count
            };
        }



        private int CalculateCompletion(
            Models.Entities.JobSeekerProfile profile,
            int skillCount)
        {
            int completed = 0;

            if (!string.IsNullOrWhiteSpace(profile.ProfessionalTitle))
                completed++;

            if (!string.IsNullOrWhiteSpace(profile.Location))
                completed++;

            if (profile.ExperienceInYears > 0)
                completed++;

            if (!string.IsNullOrWhiteSpace(profile.Education))
                completed++;

            if (!string.IsNullOrWhiteSpace(profile.About))
                completed++;

            if (skillCount > 0)
                completed++;

            return (int)Math.Round((completed / 6.0) * 100);
        }
    }
}
