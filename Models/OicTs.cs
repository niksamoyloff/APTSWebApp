using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class OicTs
    {
        public OicTs()
        {
            ReceivedTsvalues = new HashSet<ReceivedTsvalues>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRemoved { get; set; }
        public string DeviceShifr { get; set; }
        public int OicId { get; set; }
        public bool IsStatusTs { get; set; }
        public string Comment { get; set; }

        public virtual Devices DeviceShifrNavigation { get; set; }
        public virtual ICollection<ReceivedTsvalues> ReceivedTsvalues { get; set; }
    }
}
