using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class PowerObjects
    {
        public PowerObjects()
        {
            PowerObjectDevices = new HashSet<PowerObjectDevices>();
            PrimaryEquipmentPowerObjects = new HashSet<PrimaryEquipmentPowerObjects>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int PowerSystemId { get; set; }
        public bool IsRemoved { get; set; }

        public virtual PowerSystems PowerSystem { get; set; }
        public virtual ICollection<PowerObjectDevices> PowerObjectDevices { get; set; }
        public virtual ICollection<PrimaryEquipmentPowerObjects> PrimaryEquipmentPowerObjects { get; set; }
    }
}
