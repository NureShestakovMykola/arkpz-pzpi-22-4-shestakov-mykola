using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public DateTime Created { get; set; }

        // For Advice notifications
        public int? AdviceId { get; set; }
        public virtual Advice? Advice { get; set; }

        // For other notifications
        public int? Value { get; set; } // For critical temperature and water level
        public int? DeviceId { get; set; }
        public virtual Device? Device { get; set; }
    }
}
