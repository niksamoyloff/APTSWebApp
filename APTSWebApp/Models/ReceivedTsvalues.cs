using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class ReceivedTsvalues
    {
        public int Id { get; set; }
        public int OicTsid { get; set; }
        public string Quality { get; set; }
        public bool IsRemoved { get; set; }
        public int Val { get; set; }
        public DateTime Dt { get; set; }

        public virtual OicTs OicTs { get; set; }
    }
}
