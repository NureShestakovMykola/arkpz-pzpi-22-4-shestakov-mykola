namespace GrowMate.Models.Device
{
    public class CreateDeviceLogModel
    {
        public int DeviceId { get; set; }
        public int Moisture { get; set; }
        public int Temperature { get; set; }
        public int WaterLevel { get; set; }
    }
}
