using Core.Enums;
using Core.Models;

namespace GrowMate.Models.Notification
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public DateTime Created { get; set; }

        // For Advice notifications
        public string? AdviceText { get; set; }

        // For other notifications
        public int? Value { get; set; } // For critical temperature and water level
        public int? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public int? DeviceCriticalMinTemperature { get; set; }
        public int? DeviceCriticalMaxTemperature { get; set; }
    }
}
