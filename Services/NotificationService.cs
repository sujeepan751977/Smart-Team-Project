using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<Notification>> GetNotificationsAsync(int userId)
        {
            return await _notificationRepository.GetByUserIdAsync(userId);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null || notification.UserId != userId)
                return;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _notificationRepository.UpdateAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);

            foreach (var notification in notifications.Where(x => !x.IsRead))
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _notificationRepository.UpdateAsync(notification);
            }

            await _notificationRepository.SaveChangesAsync();
        }

        public async Task CreateAsync(Notification notification)
        {
            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }
    }
}