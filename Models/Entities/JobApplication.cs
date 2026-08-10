using Recruitment_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class JobApplication
    {
        public int Id { get; set; }

        public int VacancyId { get; set; }

        public int JobSeekerProfileId { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

        public decimal MatchScore { get; set; }

        [MaxLength(1000)]
        public string? CoverLetter { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public Vacancy Vacancy { get; set; } = null!;

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    }
}
