using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class Solution
    {
        public String Shaft { get; set; }
        public double SheetMetal { get; set; }
        public double Cost { get; set; }
        public double StaticPressure { get; set; }

        public List<Edge> Edges { get; set; }
    }
}
