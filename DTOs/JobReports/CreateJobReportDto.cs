using System.ComponentModel.DataAnnotations;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.DTOs.JobReports
{
    public class CreateJobReportDto
    {
        [Required]
        public JobReportReason Reason { get; set; }

        public string? Description { get; set; }
    }
}
