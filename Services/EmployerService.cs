using Recruitment_Project.DTOs.Employers;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Services
{
    public class EmployerService : IEmployerService
    {
        private readonly IEmployerRepository _employerRepository;

        public EmployerService(IEmployerRepository employerRepository)
        {
            _employerRepository = employerRepository;
        }


        public async Task<EmployerProfileDto?> GetProfileAsync(int userId)
        {
            var employerProfile = await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
            {
                return null;
            }

            return new EmployerProfileDto
            {
                Id = employerProfile.Id,
                CompanyName = employerProfile.CompanyName,
                RegisteredCompanyName = employerProfile.RegisteredCompanyName,
                RegistrationNumber = employerProfile.RegistrationNumber,
                Industry = employerProfile.Industry,
                RegisteredAddress = employerProfile.RegisteredAddress,
                OperatingLocation = employerProfile.OperatingLocation,
                OfficialCompanyEmail = employerProfile.OfficialCompanyEmail,
                CompanyPhone = employerProfile.CompanyPhone,
                Website = employerProfile.Website,
                AuthorizedRepresentative = employerProfile.AuthorizedRepresentative,
                CompanyDescription = employerProfile.CompanyDescription
            };
        }

        public async Task CreateProfileAsync(int userId, UpdateEmployerProfileDto dto)
        {
            var employerProfile = new EmployerProfile
            {
                UserId = userId,
                CompanyName = dto.CompanyName,
                RegisteredCompanyName = dto.RegisteredCompanyName,
                RegistrationNumber = dto.RegistrationNumber,
                Industry = dto.Industry,
                RegisteredAddress = dto.RegisteredAddress,
                OperatingLocation = dto.OperatingLocation,
                OfficialCompanyEmail = dto.OfficialCompanyEmail,
                CompanyPhone = dto.CompanyPhone,
                Website = dto.Website,
                AuthorizedRepresentative = dto.AuthorizedRepresentative,
                CompanyDescription = dto.CompanyDescription,
                CreatedAt = DateTime.UtcNow
            };

            await _employerRepository.AddAsync(employerProfile);
            await _employerRepository.SaveChangesAsync();
        }

        public async Task UpdateProfileAsync(int userId, UpdateEmployerProfileDto dto)
        {
            var employerProfile = await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
            {
                throw new Exception("Employer profile not found");
            }

            employerProfile.CompanyName = dto.CompanyName;
            employerProfile.RegisteredCompanyName = dto.RegisteredCompanyName;
            employerProfile.RegistrationNumber = dto.RegistrationNumber;
            employerProfile.Industry = dto.Industry;
            employerProfile.RegisteredAddress = dto.RegisteredAddress;
            employerProfile.OperatingLocation = dto.OperatingLocation;
            employerProfile.OfficialCompanyEmail = dto.OfficialCompanyEmail;
            employerProfile.CompanyPhone = dto.CompanyPhone;
            employerProfile.Website = dto.Website;
            employerProfile.AuthorizedRepresentative = dto.AuthorizedRepresentative;
            employerProfile.CompanyDescription = dto.CompanyDescription;

            await _employerRepository.UpdateAsync(employerProfile);

            await _employerRepository.SaveChangesAsync();
        }

        public async Task<EmployerDashboardDto> GetDashboardAsync(int userId)
        {
            var employerProfile = await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
            {
                throw new Exception("Employer profile not found");
            }

            return await _employerRepository.GetDashboardAsync(employerProfile.Id);
        }
    }
}