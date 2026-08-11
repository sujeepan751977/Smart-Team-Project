using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.DTOs.Common
{
    public class FeedbackRequestDto
    {
        [Required]
        public string Feedback { get; set; } = string.Empty;
    }
}
