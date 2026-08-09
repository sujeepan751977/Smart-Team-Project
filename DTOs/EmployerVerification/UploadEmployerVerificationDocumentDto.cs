using Microsoft.AspNetCore.Http;

namespace Recruitment_Project.DTOs.EmployerVerification
{
    public class UploadEmployerVerificationDocumentDto
    {
        public IFormFile File { get; set; } = null!;
    }
}