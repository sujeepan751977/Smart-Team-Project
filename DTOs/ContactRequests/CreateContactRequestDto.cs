using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.DTOs.ContactRequests
{
    public class CreateContactRequestDto
    {
        [MaxLength(1000)]
        public string? EmployerMessage { get; set; }
    }
}