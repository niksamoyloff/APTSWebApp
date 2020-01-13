using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class PrimaryEquipmentPowerObjects
    {
        public int PowerObjectId { get; set; }
        public string PrimaryEquipmentShifr { get; set; }

        public virtual PowerObjects PowerObject { get; set; }
        public virtual PrimaryEquipments PrimaryEquipmentShifrNavigation { get; set; }
    }
}
