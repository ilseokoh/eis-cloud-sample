using System;
using System.Collections.Generic;
using System.Text;

namespace EISFunctionApp
{
    public class DefectLocation
    {
        public DateTime DetectedTime { get; set; }
        public string DeviceId { get; set; }

        public string ModuleId { get; set; }

        public int DefectType { get; set; }

        public int tl_x { get; set; }

        public int tl_y { get; set; }

        public int br_x { get; set; }

        public int br_y { get; set; }
    }
}
