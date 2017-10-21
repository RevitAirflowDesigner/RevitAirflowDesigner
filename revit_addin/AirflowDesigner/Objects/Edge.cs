using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class Edge
    {
        public int Node1 { get; set; }
        public int Node2 { get; set; }
        public double Distance { get; set; }
        public double Diameter { get; set; }
        public double Airflow { get; set; }
    }
}
