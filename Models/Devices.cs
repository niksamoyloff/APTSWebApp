using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class Devices
    {
        public Devices()
        {
            OicTs = new HashSet<OicTs>();
            PowerObjectDevices = new HashSet<PowerObjectDevices>();
            PrimaryEquipmentDevices = new HashSet<PrimaryEquipmentDevices>();
        }

        public string Name { get; set; }
        public int DeviceTypeId { get; set; }
        public bool IsRemoved { get; set; }
        public string Shifr { get; set; }

        public virtual DeviceTypes DeviceType { get; set; }
        public virtual ICollection<OicTs> OicTs { get; set; }
        public virtual ICollection<PowerObjectDevices> PowerObjectDevices { get; set; }
        public virtual ICollection<PrimaryEquipmentDevices> PrimaryEquipmentDevices { get; set; }
    }
}
