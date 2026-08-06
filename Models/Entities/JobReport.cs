using Recruitment_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class JobReport
    {
        public int Id { get; set; }

        public int VacancyId { get; set; }

        public int ReportedByUserId { get; set; }

        public JobReportReason Reason { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public JobReportStatus Status { get; set; } = JobReportStatus.Pending;

        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        // Navigation Properties
        public Vacancy Vacancy { get; set; } = null!;

        public User ReportedByUser { get; set; } = null!;
    }
}
