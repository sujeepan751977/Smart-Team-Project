using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.DTOs.Vacancies
{
    public class EmployerVacancyDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string EmploymentType { get; set; } = string.Empty;

        public string WorkLocation { get; set; } = string.Empty;

        public string ExperienceLevel { get; set; } = string.Empty;

        public string SalaryRange { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Requirements { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public VacancyStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}