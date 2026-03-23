using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Services;

namespace ZuriFluxAPI.Controllers
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

        [HttpPost("device-token")]
        public async Task<IActionResult> RegisterDeviceToken(
            [FromBody] RegisterDeviceTokenDto dto)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _notificationService.RegisterDeviceTokenAsync(userId, dto);
            return Ok(new { message = "Device token registered successfully." });
        }

        [HttpPost("test")]
        public async Task<IActionResult> SendTestNotification()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            await _notificationService.SendToUserAsync(userId, new NotificationDto
            {
                Title = "🎉 ZuriFlux Test Notification",
                Body = "Push notifications are working correctly!",
                Type = "test",
                Data = new Dictionary<string, string>
                {
                    { "timestamp", DateTime.UtcNow.ToString() }
                }
            });

            return Ok(new { message = "Test notification sent." });
        }
    }
}