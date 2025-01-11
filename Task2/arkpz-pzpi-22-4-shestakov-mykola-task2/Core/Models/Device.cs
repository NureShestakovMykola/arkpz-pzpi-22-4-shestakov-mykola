using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Device
    {
        public int UserId { get; set; }
        public virtual AppUser User { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AutomaticWatering { get; set; }
        public string? ImageExtension { get; set; }

        public int CriticalMinMoisture { get; set; }
        public int CriticalMaxMoisture { get; set; }

        public int CriticalMinTemperature { get; set; }
        public int CriticalMaxTemperature { get; set; }

        public int? ScheduleId { get; set; }
        public virtual Schedule? Schedule { get; set; }

        public virtual List<WateringLog> WateringLogs { get; set; } = [];
        public virtual List<DeviceLog> DeviceLogs { get; set; } = [];
        public virtual List<ManualWateringRequest> ManualWateringRequests { get; set; } = [];
        public virtual List<Notification> Notifications { get; set; } = [];
    }
}
