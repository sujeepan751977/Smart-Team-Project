using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.DTOs.Applications
{
    public class ApplyJobDto
    {
        [MaxLength(1000)]
        public string? CoverLetter { get; set; }
    }
}