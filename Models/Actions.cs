using System;
using System.Collections.Generic;

namespace APTSWebApp.Models
{
    public partial class Actions
    {
        public int Id { get; set; }
        public DateTime Dtime { get; set; }
        public string UserName { get; set; }
        public string ActionName { get; set; }
        public string OicTsName { get; set; }
        public string PrimaryName { get; set; }
        public string DeviceName { get; set; }
        public string PowerObjectName { get; set; }
        public string TsOicId { get; set; }
    }
}
