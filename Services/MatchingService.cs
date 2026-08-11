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
            if (jobSeekerProfileId <= 0)
            {
                return new MatchResultDto();
            }

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
                .Where(x => x.Skill != null && !string.IsNullOrWhiteSpace(x.Skill.Name))
                .Select(x => x.Skill.Name.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            var jobSkills = vacancy.RequiredSkills
                .Where(x => x.Skill != null && !string.IsNullOrWhiteSpace(x.Skill.Name))
                .Select(x => x.Skill.Name.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            var matchedSkills = seekerSkills
                .Intersect(jobSkills)
                .ToList();

            var missingSkills = jobSkills
                .Except(seekerSkills)
                .ToList();

            // Skills: up to 60 points
            double skillsScore;
            if (jobSkills.Count == 0)
            {
                skillsScore = 60;
            }
            else
            {
                skillsScore =
                    ((double)matchedSkills.Count / jobSkills.Count) * 60;
            }

            // Experience: up to 20 points (compare seeker years vs vacancy requirement)
            var requiredYears = ParseExperienceYears(vacancy.ExperienceLevel);
            double experienceScore;
            if (requiredYears <= 0)
            {
                experienceScore = profile.ExperienceInYears > 0 ? 20 : 10;
            }
            else if (profile.ExperienceInYears >= requiredYears)
            {
                experienceScore = 20;
            }
            else if (profile.ExperienceInYears > 0)
            {
                experienceScore =
                    20.0 * profile.ExperienceInYears / requiredYears;
            }
            else
            {
                experienceScore = 0;
            }

            // Education: up to 10 points
            double educationScore =
                string.IsNullOrWhiteSpace(profile.Education) ? 0 : 10;

            // Location: up to 10 points
            double locationScore = 0;
            if (!string.IsNullOrWhiteSpace(profile.Location) &&
                !string.IsNullOrWhiteSpace(vacancy.WorkLocation) &&
                string.Equals(
                    profile.Location.Trim(),
                    vacancy.WorkLocation.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                locationScore = 10;
            }

            var total = Math.Round(
                skillsScore + experienceScore + educationScore + locationScore,
                1,
                MidpointRounding.AwayFromZero);

            return new MatchResultDto
            {
                TotalMatchScore = total,
                Breakdown = new MatchBreakdownDto
                {
                    SkillsScore = Math.Round(skillsScore, 1, MidpointRounding.AwayFromZero),
                    ExperienceScore = Math.Round(experienceScore, 1, MidpointRounding.AwayFromZero),
                    EducationScore = educationScore,
                    LocationScore = locationScore
                },
                MatchedSkills = matchedSkills.Select(Capitalize).ToList(),
                MissingSkills = missingSkills.Select(Capitalize).ToList()
            };
        }

        private static int ParseExperienceYears(string? experienceLevel)
        {
            if (string.IsNullOrWhiteSpace(experienceLevel))
                return 0;

            var digits = new string(
                experienceLevel.Where(char.IsDigit).ToArray());

            return int.TryParse(digits, out var years) ? years : 0;
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return char.ToUpperInvariant(value[0]) + value[1..];
        }
    }
}
