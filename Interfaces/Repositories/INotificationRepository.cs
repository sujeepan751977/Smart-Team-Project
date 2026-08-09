using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);

        Task<int> GetUnreadCountAsync(int userId);

        Task<Notification?> GetByIdAsync(int id);

        Task AddAsync(Notification notification);

        Task UpdateAsync(Notification notification);

        Task SaveChangesAsync();
    }
}