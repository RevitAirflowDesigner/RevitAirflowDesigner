using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class Network
    {
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
        public List<Space> Spaces { get; set; }
        
    }
}
