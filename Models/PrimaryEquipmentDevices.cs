using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class PrimaryEquipmentDevices
    {
        public string PrimaryEquipmentShifr { get; set; }
        public string DeviceShifr { get; set; }

        public virtual Devices DeviceShifrNavigation { get; set; }
        public virtual PrimaryEquipments PrimaryEquipmentShifrNavigation { get; set; }
    }
}
