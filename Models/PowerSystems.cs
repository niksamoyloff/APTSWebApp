using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class PowerSystems
    {
        public PowerSystems()
        {
            PowerObjects = new HashSet<PowerObjects>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
        public bool IsRemoved { get; set; }

        public virtual ICollection<PowerObjects> PowerObjects { get; set; }
    }
}
