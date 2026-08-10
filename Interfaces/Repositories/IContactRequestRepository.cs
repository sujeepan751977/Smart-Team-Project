using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IContactRequestRepository
    {
        Task<ContactRequest?> GetByIdAsync(int id);

        Task<ContactRequest?> GetPendingRequestAsync(
            int jobApplicationId);

        Task<List<ContactRequest>> GetByEmployerAsync(
            int employerProfileId);

        Task<List<ContactRequest>> GetByJobSeekerAsync(
            int jobSeekerProfileId);

        Task AddAsync(ContactRequest contactRequest);

        Task UpdateAsync(ContactRequest contactRequest);

        Task SaveChangesAsync();
    }
}