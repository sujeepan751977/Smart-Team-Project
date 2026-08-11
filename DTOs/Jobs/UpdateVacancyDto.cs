using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.DTOs.Jobs
{
    public class UpdateVacancyDto
    {
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

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Requirements { get; set; } = string.Empty;

        public List<string> RequiredSkills { get; set; } = new();

        public DateTime ExpiryDate { get; set; }
    }
}