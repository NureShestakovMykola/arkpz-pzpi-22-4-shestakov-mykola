using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class WateringLog
    {
        public int Id { get; set; }
        public DateTime LogDateTime { get; set; }

        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }
    }
}
