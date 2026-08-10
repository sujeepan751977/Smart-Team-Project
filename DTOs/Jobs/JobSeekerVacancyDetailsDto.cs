using Recruitment_Project.DTOs.Matching;

namespace Recruitment_Project.DTOs.Jobs
{
    public class JobSeekerVacancyDetailsDto
    {
        public int Id { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int RequiredExperience { get; set; }

        public string EducationRequirement { get; set; } = string.Empty;

        public List<string> RequiredSkills { get; set; } = new();

        public double MatchScore { get; set; }

        public MatchBreakdownDto MatchBreakdown { get; set; } = new();

        public List<string> MatchedSkills { get; set; } = new();

        public List<string> MissingSkills { get; set; } = new();

        public bool CanApply { get; set; }

        public string TrustLabel { get; set; } = string.Empty;
    }
}
