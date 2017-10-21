using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class Edge
    {
        public Node Pt0 { get; set; }
        public Node Pt1 { get; set; }

        public double Distance { get; set; }

        public double Airflow { get; set; }
    }
}
