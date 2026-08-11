using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.DTOs.Common
{
    public class ReasonRequestDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
