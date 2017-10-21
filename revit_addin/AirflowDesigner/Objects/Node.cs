using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class Node
    {
        private static int COUNTER = 0;
        public enum NodeTypeEnum { Vav, Shaft, Other };

        public int Id { get; private set; }
        public string Name { get; set; }

        public NodeTypeEnum NodeType { get; set; }

        public XYZ Location { get; set; }

        public String SpaceId { get; set; }

        public Node()
        {
            COUNTER++;
            Id = COUNTER;
        }
        public override string ToString()
        {
            return NodeType + ": " + Id + ": " + Name;
        }
    }
}
