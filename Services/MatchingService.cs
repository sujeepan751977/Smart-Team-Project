using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.DTOs.Matching;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly AppDbContext _context;

        public MatchingService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<MatchResultDto> CalculateMatchAsync(
            int jobSeekerProfileId,
            int vacancyId)
        {
            var profile = await _context.JobSeekerProfiles
                .Include(x => x.Skills)
                .ThenInclude(x => x.Skill)
                .FirstOrDefaultAsync(x => x.Id == jobSeekerProfileId);


            var vacancy = await _context.Vacancies
                .Include(x => x.RequiredSkills)
                .ThenInclude(x => x.Skill)
                .FirstOrDefaultAsync(x => x.Id == vacancyId);


            if (profile == null || vacancy == null)
            {
                return new MatchResultDto();
            }


            var seekerSkills = profile.Skills
                .Select(x => x.Skill.Name.ToLower())
                .ToList();


            var jobSkills = vacancy.RequiredSkills
                .Select(x => x.Skill.Name.ToLower())
                .ToList();


            var matchedSkills = seekerSkills
                .Intersect(jobSkills)
                .ToList();


            var missingSkills = jobSkills
                .Except(seekerSkills)
                .ToList();


            double skillsScore = 0;

            if (jobSkills.Count > 0)
            {
                skillsScore =
                    ((double)matchedSkills.Count / jobSkills.Count) * 60;
            }


            double experienceScore = 0;

            if (profile.ExperienceInYears > 0)
            {
                experienceScore = 20;
            }


            double educationScore = 0;

            if (!string.IsNullOrEmpty(profile.Education))
            {
                educationScore = 10;
            }


            double locationScore = 0;

            if (profile.Location == vacancy.WorkLocation)
            {
                locationScore = 10;
            }


            return new MatchResultDto
            {
                TotalMatchScore =
                    skillsScore +
                    experienceScore +
                    educationScore +
                    locationScore,


                Breakdown = new MatchBreakdownDto
                {
                    SkillsScore = skillsScore,

                    ExperienceScore = experienceScore,

                    EducationScore = educationScore,

                    LocationScore = locationScore
                },


                MatchedSkills = matchedSkills,

                MissingSkills = missingSkills
            };
        }
    }
}