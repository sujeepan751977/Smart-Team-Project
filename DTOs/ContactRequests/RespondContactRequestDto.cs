using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.DTOs.ContactRequests
{
    public class RespondContactRequestDto
    {
        [Required]
        public string Response { get; set; } = string.Empty;
    }
}
