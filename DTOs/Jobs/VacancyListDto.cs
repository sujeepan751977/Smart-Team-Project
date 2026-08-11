namespace Recruitment_Project.DTOs.Jobs
{
    public class VacancyListDto
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

        /// <summary>
        /// True when score was calculated for the signed-in job seeker.
        /// </summary>
        public bool HasMatchScore { get; set; }
    }
}
