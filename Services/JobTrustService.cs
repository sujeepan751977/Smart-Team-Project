using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class JobTrustService : IJobTrustService
    {
        private readonly AppDbContext _context;

        public JobTrustService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetTrustLabelAsync(int vacancyId)
        {
            var vacancy = await _context.Vacancies
                .Include(v => v.EmployerProfile)
                    .ThenInclude(e => e.Verification)
                .FirstOrDefaultAsync(v => v.Id == vacancyId);

            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found.");

            var employer = vacancy.EmployerProfile;

            // Disabled employer
            if (employer.AccountStatus == EmployerAccountStatus.Disabled)
                return "Restricted";

            // Suspended employer
            if (employer.AccountStatus == EmployerAccountStatus.Suspended)
                return "Restricted";

            // Vacancy not approved/open
            if (vacancy.Status != VacancyStatus.Open)
                return "Under Review";

            // Employer verification check
            if (employer.Verification == null ||
                employer.Verification.Status != EmployerVerificationStatus.Verified)
            {
                return "Standard Review";
            }

            // Official company email check
            if (string.IsNullOrWhiteSpace(employer.OfficialCompanyEmail))
                return "Standard Review";

            // All main trust conditions satisfied
            return "Verified";
        }
    }
}