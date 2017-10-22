using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class Solution
    {
        public int Id { get; set; }
        public String Shaft { get; set; }
        public double SheetMetal { get; set; }
        public double Cost { get; set; }
        public double StaticPressure { get; set; }

        public List<Edge> Edges { get; set; }

        public Solution()
        {
            Edges = new List<Edge>();
        }

        public IList<Edge> GetCorridorEdges(IList<Node> nodes)
        {
            List<Edge> outEdges = new List<Edge>();
            foreach(var edge in Edges)
            {
                Node n1 = nodes.Single(n => n.Id == edge.Node1);
                Node n2 = nodes.Single(n => n.Id == edge.Node2);

                if ((n1.NodeType == Node.NodeTypeEnum.Other) && (n2.NodeType == Node.NodeTypeEnum.Other)) outEdges.Add(edge);
            }

            return outEdges;
        }

        public IList<Edge> GetVAVEdges(IList<Node> nodes)
        {
            List<Edge> outEdges = new List<Edge>();
            foreach (var edge in Edges)
            {
                Node n1 = nodes.Single(n => n.Id == edge.Node1);
                Node n2 = nodes.Single(n => n.Id == edge.Node2);

                if ((n1.NodeType == Node.NodeTypeEnum.Vav) || (n2.NodeType == Node.NodeTypeEnum.Vav)) outEdges.Add(edge);
            }

            return outEdges;
        }

        public IList<Edge> GetShaftEdges(IList<Node> nodes)
        {
            List<Edge> outEdges = new List<Edge>();
            foreach (var edge in Edges)
            {
                Node n1 = nodes.Single(n => n.Id == edge.Node1);
                Node n2 = nodes.Single(n => n.Id == edge.Node2);

                if ((n1.NodeType == Node.NodeTypeEnum.Shaft) || (n2.NodeType == Node.NodeTypeEnum.Shaft)) outEdges.Add(edge);
            }

            return outEdges;
        }
    }
}
