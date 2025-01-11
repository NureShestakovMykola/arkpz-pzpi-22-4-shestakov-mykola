namespace GrowMate.Models.Device
{
    public class EditDeviceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int CriticalMinMoisture { get; set; }
        public int CriticalMaxMoisture { get; set; }

        public int CriticalMinTemperature { get; set; }
        public int CriticalMaxTemperature { get; set; }

        public int? ScheduleId { get; set; }

        public IFormFile? NewImage { get; set; }
    }
}
