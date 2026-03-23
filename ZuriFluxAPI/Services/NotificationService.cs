using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.Data;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ZuriFluxDbContext _context;

        public NotificationService(ZuriFluxDbContext context)
        {
            _context = context;

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential
                        .FromFile("firebase-credentials.json")
                });
            }
        }

        public async Task RegisterDeviceTokenAsync(
            int userId, RegisterDeviceTokenDto dto)
        {
            var existing = await _context.DeviceTokens
                .FirstOrDefaultAsync(d =>
                    d.UserId == userId && d.Token == dto.Token);

            if (existing != null)
            {
                existing.LastUsed = DateTime.UtcNow;
                _context.DeviceTokens.Update(existing);
            }
            else
            {
                _context.DeviceTokens.Add(new DeviceToken
                {
                    UserId = userId,
                    Token = dto.Token,
                    Platform = dto.Platform,
                    RegisteredAt = DateTime.UtcNow,
                    LastUsed = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task SendToUserAsync(int userId, NotificationDto notification)
        {
            var tokens = await _context.DeviceTokens
                .Where(d => d.UserId == userId)
                .Select(d => d.Token)
                .ToListAsync();

            if (!tokens.Any()) return;
            await SendToTokensAsync(tokens, notification);
        }

        public async Task SendToRoleAsync(string role, NotificationDto notification)
        {
            var tokens = await _context.DeviceTokens
                .Include(d => d.User)
                .Where(d => d.User.Role == role)
                .Select(d => d.Token)
                .ToListAsync();

            if (!tokens.Any()) return;
            await SendToTokensAsync(tokens, notification);
        }

        public async Task SendBinAlertAsync(Bin bin)
        {
            await SendToRoleAsync("collector", new NotificationDto
            {
                Title = "🚨 Bin Alert — Pickup Needed!",
                Body = $"Bin at {bin.Location} is {bin.FillLevel}% full.",
                Type = "bin_alert",
                Data = new Dictionary<string, string>
                {
                    { "binId", bin.Id.ToString() },
                    { "location", bin.Location },
                    { "fillLevel", bin.FillLevel.ToString() },
                    { "latitude", bin.Latitude.ToString() },
                    { "longitude", bin.Longitude.ToString() }
                }
            });
        }

        public async Task SendScheduleReminderAsync(CollectionSchedule schedule)
        {
            if (schedule.AssignedCollectorId == null) return;

            await SendToUserAsync(schedule.AssignedCollectorId.Value,
                new NotificationDto
                {
                    Title = "📅 Pickup Reminder",
                    Body = $"Scheduled pickup at {schedule.Bin?.Location} — " +
                           $"{schedule.TimeSlot} on {schedule.ScheduledDate:MMM dd}.",
                    Type = "schedule_reminder",
                    Data = new Dictionary<string, string>
                    {
                        { "scheduleId", schedule.Id.ToString() },
                        { "binId", schedule.BinId.ToString() },
                        { "timeSlot", schedule.TimeSlot },
                        { "scheduledDate", schedule.ScheduledDate.ToString("yyyy-MM-dd") }
                    }
                });
        }

        private async Task SendToTokensAsync(
            List<string> tokens, NotificationDto notification)
        {
            var batches = tokens
                .Select((token, index) => new { token, index })
                .GroupBy(x => x.index / 500)
                .Select(g => g.Select(x => x.token).ToList());

            foreach (var batch in batches)
            {
                var message = new MulticastMessage
                {
                    Tokens = batch,
                    Notification = new Notification
                    {
                        Title = notification.Title,
                        Body = notification.Body
                    },
                    Data = notification.Data,
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            Sound = "default"
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "default",
                            Badge = 1
                        }
                    }
                };

                await FirebaseMessaging.DefaultInstance
                    .SendEachForMulticastAsync(message);
            }
        }
    }
}