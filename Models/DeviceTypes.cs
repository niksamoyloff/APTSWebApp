using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class DeviceTypes
    {
        public DeviceTypes()
        {
            Devices = new HashSet<Devices>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRemoved { get; set; }

        public virtual ICollection<Devices> Devices { get; set; }
    }
}
