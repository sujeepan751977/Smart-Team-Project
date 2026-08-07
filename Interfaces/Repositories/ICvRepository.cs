using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface ICvRepository
    {
        Task AddAsync(CvDocument cvDocument);

        Task<CvDocument?> GetByUserIdAsync(int userId);

        Task SaveChangesAsync();
    }
}