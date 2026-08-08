namespace Recruitment_Project.DTOs.Jobs
{
    public class JobSearchRequestDto
    {
        public string? Search { get; set; }

        public string? Company { get; set; }

        public string? Skill { get; set; }

        public string? Location { get; set; }

        public int? MinimumExperience { get; set; }

        public double? MinimumMatch { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
