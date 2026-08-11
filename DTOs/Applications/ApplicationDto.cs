namespace Recruitment_Project.DTOs.Applications
{
    public class ApplicationDto
    {
        public int Id { get; set; }

        public int VacancyId { get; set; }

        public int JobSeekerProfileId { get; set; }

        public string? CandidateName { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal MatchScore { get; set; }

        public string? CoverLetter { get; set; }

        public bool HasCv { get; set; }

        public string? CvFileName { get; set; }

        public DateTime AppliedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}