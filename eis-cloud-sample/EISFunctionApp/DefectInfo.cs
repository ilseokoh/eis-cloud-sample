using System;
using System.Collections.Generic;
using System.Text;

namespace EISFunctionApp
{
    public class DefectInfo
    {
        public int frame_number { get; set; }
        public int channels { get; set; }
        public string encoding_type { get; set; }
        public int height { get; set; }
        public Defect[] defects { get; set; }
        public string img_handle { get; set; }
        public int width { get; set; }
        public int encoding_level { get; set; }
    }

    public class Defect
    {
        public int type { get; set; }
        public int[] tl { get; set; }
        public int[] br { get; set; }
    }

}
