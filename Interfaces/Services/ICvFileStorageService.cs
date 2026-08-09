using Microsoft.AspNetCore.Http;
using Recruitment_Project.DTOs.JobSeekers;

namespace Recruitment_Project.Interfaces.Services
{
    public interface ICvFileStorageService
    {
        Task<CvDocumentDto> UploadCvAsync(
            int userId,
            IFormFile file);

        Task<CvDocumentDto?> GetCvAsync(
            int userId);

        Task<bool> ReplaceCvAsync(
            int userId,
            IFormFile file);
    }
}