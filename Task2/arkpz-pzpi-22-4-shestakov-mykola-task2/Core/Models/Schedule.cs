using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Schedule
    {
        public int UserId { get; set; }
        public virtual AppUser User { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public DateTime WateringTime { get; set; }
        public ScheduleType ScheduleType { get; set; }

        public int? DaysGap { get; set; }
        public int? Days { get; set; }
        //public List<WeeklyWateringDay> WateringDays { get; set; } = [];

        public virtual List<Device> Devices { get; set; } = [];
    }
}
