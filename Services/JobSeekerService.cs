using Recruitment_Project.DTOs.JobSeekers;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;

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


            profile.ProfileCompletionPercentage =
                CalculateCompletion(profile);



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
            Models.Entities.JobSeekerProfile profile)
        {
            int completed = 0;

            int total = 5;


            if (!string.IsNullOrEmpty(profile.ProfessionalTitle))
                completed++;

            if (!string.IsNullOrEmpty(profile.Location))
                completed++;

            if (profile.ExperienceInYears > 0)
                completed++;

            if (!string.IsNullOrEmpty(profile.Education))
                completed++;

            if (!string.IsNullOrEmpty(profile.About))
                completed++;


            return (completed * 100) / total;
        }
    }
}