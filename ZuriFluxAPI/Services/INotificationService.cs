using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Services
{
    public interface INotificationService
    {
        Task RegisterDeviceTokenAsync(int userId, RegisterDeviceTokenDto dto);
        Task SendToUserAsync(int userId, NotificationDto notification);
        Task SendToRoleAsync(string role, NotificationDto notification);
        Task SendBinAlertAsync(Bin bin);
        Task SendScheduleReminderAsync(CollectionSchedule schedule);
    }
}