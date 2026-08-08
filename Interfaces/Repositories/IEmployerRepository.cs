using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IEmployerRepository
    {
        Task<EmployerProfile?> GetByUserIdAsync(int userId);

        Task AddAsync(EmployerProfile employerProfile);

        Task UpdateAsync(EmployerProfile employerProfile);

        Task SaveChangesAsync();
    }
}
