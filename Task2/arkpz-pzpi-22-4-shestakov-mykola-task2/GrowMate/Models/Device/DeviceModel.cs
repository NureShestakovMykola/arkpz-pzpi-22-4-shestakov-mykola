namespace GrowMate.Models.Device
{
    public class DeviceModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AutomaticWatering { get; set; }

        public int CriticalMinMoisture { get; set; }
        public int CriticalMaxMoisture { get; set; }

        public int CriticalMinTemperature { get; set; }
        public int CriticalMaxTemperature { get; set; }

        public int? ScheduleId { get; set; }

        public int WaterLevel { get; set; }
        public DateTime? LastWatering { get; set; }
    }
}
