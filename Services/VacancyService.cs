using Recruitment_Project.DTOs.Jobs;
using Recruitment_Project.DTOs.Vacancies;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class VacancyService : IVacancyService
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IEmployerRepository _employerRepository;
        private readonly IEmployerVerificationRepository _verificationRepository;

        public VacancyService(
            IVacancyRepository vacancyRepository,
            IEmployerRepository employerRepository,
            IEmployerVerificationRepository verificationRepository)
        {
            _vacancyRepository = vacancyRepository;
            _employerRepository = employerRepository;
            _verificationRepository = verificationRepository;
        }

     
        // Get all vacancies of the current employer
      
        public async Task<List<EmployerVacancyDto>> GetMyVacanciesAsync(
            int userId)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
                throw new Exception("Employer profile not found");

            var vacancies =
                await _vacancyRepository
                    .GetByEmployerProfileIdAsync(employer.Id);

            return vacancies
                .Select(MapToDto)
                .ToList();
        }

        // ---------------------------------------------------------
        // Get one vacancy of the current employer
        // ---------------------------------------------------------
        public async Task<EmployerVacancyDto?> GetMyVacancyByIdAsync(
            int userId,
            int vacancyId)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
                throw new Exception("Employer profile not found");

            var vacancy =
                await _vacancyRepository.GetByIdAsync(vacancyId);

            if (vacancy == null ||
                vacancy.EmployerProfileId != employer.Id)
            {
                return null;
            }

            return MapToDto(vacancy);
        }

       
        // Create vacancy
        // Suspended employers cannot create vacancies
        
        public async Task<int> CreateVacancyAsync(
            int userId,
            CreateVacancyDto dto)
        {
            Console.WriteLine("TOKEN USER ID = " + userId);

            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
                throw new Exception("Employer profile not found");

            if (employer.AccountStatus ==
                EmployerAccountStatus.Suspended)
            {
                throw new Exception(
                    "Suspended employers cannot create vacancies");
            }

            var vacancy = new Vacancy
            {
                EmployerProfileId = employer.Id,
                Title = dto.Title,
                Category = dto.Category,
                EmploymentType = dto.EmploymentType,
                WorkLocation = dto.WorkLocation,
                ExperienceLevel = dto.ExperienceLevel,
                SalaryRange = dto.SalaryRange,
                Description = dto.Description,
                Requirements = dto.Requirements,
                ExpiryDate = dto.ExpiryDate,
                Status = VacancyStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            await _vacancyRepository.AddAsync(vacancy);
            await _vacancyRepository.SaveChangesAsync();

            return vacancy.Id;
        }

        
        // Update vacancy
        // Suspended employers cannot update vacancies
       
        public async Task UpdateVacancyAsync(
            int userId,
            int vacancyId,
            UpdateVacancyDto dto)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
                throw new Exception("Employer profile not found");

            if (employer.AccountStatus ==
                EmployerAccountStatus.Suspended)
            {
                throw new Exception(
                    "Suspended employers cannot update vacancies");
            }

            var vacancy =
                await _vacancyRepository.GetByIdAsync(vacancyId);

            if (vacancy == null ||
                vacancy.EmployerProfileId != employer.Id)
            {
                throw new Exception("Vacancy not found");
            }

            if (vacancy.Status != VacancyStatus.Draft &&
                vacancy.Status != VacancyStatus.Rejected)
            {
                throw new Exception(
                    "Only draft or rejected vacancies can be updated");
            }

            vacancy.Title = dto.Title;
            vacancy.Category = dto.Category;
            vacancy.EmploymentType = dto.EmploymentType;
            vacancy.WorkLocation = dto.WorkLocation;
            vacancy.ExperienceLevel = dto.ExperienceLevel;
            vacancy.SalaryRange = dto.SalaryRange;
            vacancy.Description = dto.Description;
            vacancy.Requirements = dto.Requirements;
            vacancy.ExpiryDate = dto.ExpiryDate;
            vacancy.UpdatedAt = DateTime.UtcNow;

            await _vacancyRepository.UpdateAsync(vacancy);
            await _vacancyRepository.SaveChangesAsync();
        }

        // ---------------------------------------------------------
        // Submit vacancy
        // Suspended employers cannot submit vacancies
        // ---------------------------------------------------------
        public async Task SubmitVacancyAsync(
            int userId,
            int vacancyId)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
                throw new Exception("Employer profile not found");

            if (employer.AccountStatus ==
                EmployerAccountStatus.Suspended)
            {
                throw new Exception(
                    "Suspended employers cannot submit vacancies");
            }

            var vacancy =
                await _vacancyRepository.GetByIdAsync(vacancyId);

            if (vacancy == null ||
                vacancy.EmployerProfileId != employer.Id)
            {
                throw new Exception("Vacancy not found");
            }

            if (vacancy.Status != VacancyStatus.Draft &&
                vacancy.Status != VacancyStatus.Rejected)
            {
                throw new Exception(
                    "Only draft or rejected vacancies can be submitted");
            }

            var verification =
                await _verificationRepository
                    .GetByEmployerProfileIdAsync(employer.Id);

            if (verification?.Status ==
                EmployerVerificationStatus.Verified)
            {
                vacancy.Status = VacancyStatus.Open;
            }
            else
            {
                vacancy.Status = VacancyStatus.PendingApproval;
            }

            vacancy.UpdatedAt = DateTime.UtcNow;

            await _vacancyRepository.UpdateAsync(vacancy);
            await _vacancyRepository.SaveChangesAsync();
        }

        // ---------------------------------------------------------
        // Close vacancy
        // Suspended employers cannot close vacancies
        // ---------------------------------------------------------
        public async Task CloseVacancyAsync(
            int userId,
            int vacancyId)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
                throw new Exception("Employer profile not found");

            if (employer.AccountStatus ==
                EmployerAccountStatus.Suspended)
            {
                throw new Exception(
                    "Suspended employers cannot close vacancies");
            }

            var vacancy =
                await _vacancyRepository.GetByIdAsync(vacancyId);

            if (vacancy == null ||
                vacancy.EmployerProfileId != employer.Id)
            {
                throw new Exception("Vacancy not found");
            }

            if (vacancy.Status != VacancyStatus.Open)
            {
                throw new Exception(
                    "Only open vacancies can be closed");
            }

            vacancy.Status = VacancyStatus.Closed;
            vacancy.UpdatedAt = DateTime.UtcNow;

            await _vacancyRepository.UpdateAsync(vacancy);
            await _vacancyRepository.SaveChangesAsync();
        }

        // ---------------------------------------------------------
        // Mapping
        // ---------------------------------------------------------
        private static EmployerVacancyDto MapToDto(
            Vacancy vacancy)
        {
            return new EmployerVacancyDto
            {
                Id = vacancy.Id,
                Title = vacancy.Title,
                Category = vacancy.Category,
                EmploymentType = vacancy.EmploymentType,
                WorkLocation = vacancy.WorkLocation,
                ExperienceLevel = vacancy.ExperienceLevel,
                SalaryRange = vacancy.SalaryRange,
                Description = vacancy.Description,
                Requirements = vacancy.Requirements,
                ExpiryDate = vacancy.ExpiryDate,
                Status = vacancy.Status,
                CreatedAt = vacancy.CreatedAt,
                UpdatedAt = vacancy.UpdatedAt
            };
        }
    }
}