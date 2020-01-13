using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class PrimaryEquipments
    {
        public PrimaryEquipments()
        {
            PrimaryEquipmentDevices = new HashSet<PrimaryEquipmentDevices>();
            PrimaryEquipmentPowerObjects = new HashSet<PrimaryEquipmentPowerObjects>();
        }

        public string Shifr { get; set; }
        public string Name { get; set; }
        public bool IsRemoved { get; set; }
        public int ZvkId { get; set; }

        public virtual ICollection<PrimaryEquipmentDevices> PrimaryEquipmentDevices { get; set; }
        public virtual ICollection<PrimaryEquipmentPowerObjects> PrimaryEquipmentPowerObjects { get; set; }
    }
}
