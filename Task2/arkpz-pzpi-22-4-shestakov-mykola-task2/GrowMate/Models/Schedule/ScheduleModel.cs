using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace GrowMate.Models.Schedule
{
    public class ScheduleModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public DateTime WateringTime { get; set; }
        public ScheduleType ScheduleType { get; set; }

        public int? DaysGap { get; set; }
        public List<int> Days { get; set; } = [];
    }
}
