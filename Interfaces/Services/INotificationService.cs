using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetNotificationsAsync(int userId);

        Task<int> GetUnreadCountAsync(int userId);

        Task MarkAsReadAsync(int notificationId, int userId);

        Task MarkAllAsReadAsync(int userId);

        Task CreateAsync(Notification notification);
    }
}