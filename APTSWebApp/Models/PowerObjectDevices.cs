using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class PowerObjectDevices
    {
        public int PowerObjectId { get; set; }
        public string DeviceShifr { get; set; }

        public virtual Devices DeviceShifrNavigation { get; set; }
        public virtual PowerObjects PowerObject { get; set; }
    }
}
