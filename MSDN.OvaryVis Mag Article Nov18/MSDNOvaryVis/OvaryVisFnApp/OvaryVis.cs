using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OvaryVisFnApp
{
    public class OvaryVis
    {
        public string Id { get; set; }
        public int D1mm { get; set; }
        public int D2mm { get; set; }
        public int D3mm { get; set; }
        public DateTime JobSubmitted { get; set; }
        public int ResultVis { get; set; }
        public string StatusMsg { get; set; }
    }
}
