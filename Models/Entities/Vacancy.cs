using Recruitment_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class Vacancy
    {
        public int Id { get; set; }

        public int EmployerProfileId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(100)]
        public string EmploymentType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string WorkLocation { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ExperienceLevel { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SalaryRange { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Requirements { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public VacancyStatus Status { get; set; } = VacancyStatus.PendingApproval;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public EmployerProfile EmployerProfile { get; set; } = null!;

        public ICollection<VacancySkill> RequiredSkills { get; set; } = new List<VacancySkill>();

        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
