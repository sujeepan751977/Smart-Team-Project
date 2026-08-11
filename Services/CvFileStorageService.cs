using Microsoft.AspNetCore.Http;
using Recruitment_Project.DTOs.JobSeekers;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Services
{
    public class CvFileStorageService : ICvFileStorageService
    {
        private readonly string _uploadPath;
        private readonly ICvRepository _cvRepository;
        private readonly IJobSeekerRepository _jobSeekerRepository;


        public CvFileStorageService(
            ICvRepository cvRepository,
            IJobSeekerRepository jobSeekerRepository)
        {
            _cvRepository = cvRepository;
            _jobSeekerRepository = jobSeekerRepository;

            _uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "CVStorage");
        }



        public async Task<CvDocumentDto> UploadCvAsync(
            int userId,
            IFormFile file)
        {
            ValidateFile(file);


            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }


            var fileName =
                Guid.NewGuid().ToString()
                + Path.GetExtension(file.FileName);


            var filePath =
                Path.Combine(_uploadPath, fileName);


            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var profile =
                await _jobSeekerRepository
                .GetProfileByUserIdAsync(userId);


            if (profile == null)
            {
                throw new KeyNotFoundException("Job seeker profile not found");
            }


            var cvDocument = new CvDocument
            {
                JobSeekerProfileId = profile.Id,

                FileName = file.FileName,

                FilePath = filePath,

                ContentType = file.ContentType,

                FileSize = file.Length,

                UploadedAt = DateTime.UtcNow
            };


            await _cvRepository.AddAsync(cvDocument);

            await _cvRepository.SaveChangesAsync();


            return new CvDocumentDto
            {
                Id = cvDocument.Id,

                FileName = cvDocument.FileName,

                FileType = cvDocument.ContentType,

                FileSize = cvDocument.FileSize,

                UploadedAt = cvDocument.UploadedAt
            };
        }



        public async Task<CvDocumentDto?> GetCvAsync(int userId)
        {
            var cv =
                await _cvRepository
                .GetByUserIdAsync(userId);


            if (cv == null)
                return null;


            return new CvDocumentDto
            {
                Id = cv.Id,

                FileName = cv.FileName,

                FileType = cv.ContentType,

                FileSize = cv.FileSize,

                UploadedAt = cv.UploadedAt
            };
        }



        public async Task<bool> ReplaceCvAsync(
            int userId,
            IFormFile file)
        {
            ValidateFile(file);

            var existingDocuments =
                await _cvRepository.GetAllByUserIdAsync(userId);

            foreach (var existing in existingDocuments)
            {
                if (!string.IsNullOrWhiteSpace(existing.FilePath) &&
                    File.Exists(existing.FilePath))
                {
                    File.Delete(existing.FilePath);
                }

                await _cvRepository.DeleteAsync(existing);
            }

            await _cvRepository.SaveChangesAsync();

            await UploadCvAsync(userId, file);

            return true;
        }



        private void ValidateFile(IFormFile file)
        {
            if (file == null)
            {
                throw new InvalidOperationException(
                    "CV file is required");
            }


            var allowedExtensions =
                new[]
                {
                    ".pdf",
                    ".doc",
                    ".docx"
                };


            var extension =
                Path.GetExtension(file.FileName)
                .ToLower();



            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Only PDF, DOC, DOCX files allowed");
            }



            if (file.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException(
                    "Maximum file size is 5MB");
            }
        }
    }
}
