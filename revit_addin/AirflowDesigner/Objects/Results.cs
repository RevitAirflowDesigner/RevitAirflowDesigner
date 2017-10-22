using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class Results
    {
        public List<Solution> Solutions { get; set; }
        public List<Node> Nodes { get; set; }

        public Results()
        {
            Solutions = new List<Solution>();
            Nodes = new List<Node>();
        }
    }
}
