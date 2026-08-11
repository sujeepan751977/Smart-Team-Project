using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.DTOs.Applications
{
    public class UpdateApplicationStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
