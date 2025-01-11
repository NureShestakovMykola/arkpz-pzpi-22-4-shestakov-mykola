using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class ManualWateringRequest
    {
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }

        public int Id { get; set; }
        public int Duration { get; set; }
        public DateTime Created { get; set; }
    }
}
