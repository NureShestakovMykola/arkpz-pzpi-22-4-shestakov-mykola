using Core.Enums;

namespace Core.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public string? ImageExtension { get; set; }

        public virtual List<Device> Devices { get; set; } = [];
        public virtual List<Schedule> Schedules { get; set; } = [];
    }
}
