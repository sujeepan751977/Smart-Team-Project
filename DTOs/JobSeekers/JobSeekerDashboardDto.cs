namespace Recruitment_Project.DTOs.JobSeekers
{
    public class JobSeekerDashboardDto
    {
        public string FullName { get; set; } = string.Empty;

        public string ProfessionalTitle { get; set; } = string.Empty;

        public int ProfileCompletion { get; set; }

        public int TotalSkills { get; set; }

        public int TotalSuitableJobs { get; set; }

        public List<string> RecommendedJobTitles { get; set; } = new();
    }
}
