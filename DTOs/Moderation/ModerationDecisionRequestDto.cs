using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.DTOs.Moderation
{
    public class ModerationDecisionRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string DecisionNote { get; set; } = string.Empty;
    }
}