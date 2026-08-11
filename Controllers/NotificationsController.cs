using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using System.Security.Claims;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            var result = await _notificationService
                .GetNotificationsAsync(userId.Value);

            return Ok(result);
        }


        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            var count = await _notificationService
                .GetUnreadCountAsync(userId.Value);

            return Ok(count);
        }


        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            await _notificationService
                .MarkAsReadAsync(id, userId.Value);

            return Ok();
        }


        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            await _notificationService
                .MarkAllAsReadAsync(userId.Value);

            return Ok();
        }


        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                return null;

            if (!int.TryParse(claim.Value, out var userId))
                return null;

            return userId;
        }
    }
}