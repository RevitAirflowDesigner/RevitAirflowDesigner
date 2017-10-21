using Autodesk.Revit.Creation;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;


namespace AirflowDesigner.Controllers
{
    public class Controller
    {
        #region Declarations
        private UIDocument _uiDoc;
        #endregion

        #region Constructor
        public Controller( UIDocument doc)
        {
            _uiDoc = doc;
        }
        #endregion

        #region PublicMethods

        public Objects.Network BuildNetwork( IList<Objects.Space> spaces, IList<FamilyInstance> VAVs, IList<FamilyInstance> shafts, IList<Line> corridorLines )
        {
            // here we want to start with a node for every VAV, linked to the appropriate space.
            // then project the VAV onto the corridor lines.
            // then build the edges...

            Objects.Network network = new Objects.Network();
            List<Objects.Node> nodes = new List<Objects.Node>();
            List<Objects.Edge> edges = new List<Objects.Edge>();
            network.Edges = edges;
            network.Nodes = nodes;
            

            // figure out the current phase.
            Phase phase = _uiDoc.Document.GetElement(_uiDoc.ActiveGraphicalView.get_Parameter(BuiltInParameter.VIEW_PHASE).AsElementId()) as Phase;

            foreach( var vav in VAVs)
            {
                XYZ location = (vav.Location as LocationPoint).Point;

                Objects.Node n = new Objects.Node() { Location = location, Name = "VAV-" + vav.Id.IntegerValue, NodeType = Objects.Node.NodeTypeEnum.Vav };
                nodes.Add(n);

                // determine the related space.
                var relatedSpace = vav.get_Space(phase);
                if (relatedSpace != null) n.SpaceId = relatedSpace.UniqueId;

                // while we are at it, get the connection point to the corridor
                XYZ connection = getClosest(location, corridorLines, true);

                // does this node already exist?
                Objects.Node connNode = lookupExisting(location, nodes);

                if (connNode == null)
                {
                    // make a new one.
                    connNode = new Objects.Node() { NodeType = Objects.Node.NodeTypeEnum.Other, Name = "Corridor", Location = connection };
                    nodes.Add(connNode);
                }

                // make an edge from VAV to corridor.
                Objects.Edge edge = new Objects.Edge() { Node1 = n.Id, Node2 = connNode.Id, Distance = n.Location.DistanceTo(connNode.Location) };
                edges.Add(edge);

            }

            // now let's do the same thing with the shaft.
            foreach( var shaft in shafts)
            {
                XYZ location = (shaft.Location as LocationPoint).Point;

                // shaft name is based on the mark
                Parameter mark = shaft.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

                Objects.Node n = new Objects.Node() { Location = location, Name = "Shaft-" };

            }

            // then we need to connect the corridor nodes 
            foreach( var cl in corridorLines )
            {
                // for each centerline, we want the endpoints of the centerline, and an ordered list of edges that cover it (including all of the midpoints from nodes).

                IList<Objects.Node> onLine = getNodesOnLine(cl, nodes);

                // see if we have to add the endpoints, or if they're already there.
                Objects.Node n1 = lookupExisting(cl.GetEndPoint(0), nodes);
                Objects.Node n2 = lookupExisting(cl.GetEndPoint(1), nodes);
                if (n1 == null)
                {
                    n1 = new Objects.Node() { Location = cl.GetEndPoint(0), NodeType = Objects.Node.NodeTypeEnum.Other, Name = "CorridorEnd" };
                    onLine.Add(n1);
                    nodes.Add(n1);
                }
                if (n2 == null)
                {
                    n2 = new Objects.Node() { Location = cl.GetEndPoint(1), NodeType = Objects.Node.NodeTypeEnum.Other, Name = "CorridorEnd" };
                    onLine.Add(n2);
                    nodes.Add(n2);
                }

                // now we want to sort these things based on the distance from n1.
                onLine = onLine.OrderBy(n => n.Location.DistanceTo(n1.Location)).ToList();

                // make edges between each thing.
                for (int i = 1; i < onLine.Count; i++)
                {
                    Objects.Edge corrEdge = new Objects.Edge() { Node1 = onLine[i - 1].Id, Node2 = onLine[i].Id, Distance = onLine[i - 1].Location.DistanceTo(onLine[i].Location) };
                    edges.Add(corrEdge);
                }

            }



            return network;

        }

        private IList<Objects.Node> getNodesOnLine(Line cl, IList<Objects.Node> nodes)
        {
            // find all of the nodes that are on the given line.
            List<Objects.Node> onLine = new List<Objects.Node>();

            foreach( var node in nodes )
            {
                var result = cl.Project(node.Location);
                if ((result != null) && (result.Distance == 0)) onLine.Add(node);
            }

            return onLine;
        }

        public IList<Objects.Space> GetAllSpaces()
        {
            // get representations of all spaces in the current view.
            FilteredElementCollector coll = new FilteredElementCollector(_uiDoc.Document, _uiDoc.ActiveGraphicalView.Id);

            coll.OfCategory(BuiltInCategory.OST_MEPSpaces);

            List<Objects.Space> spaces = new List<Objects.Space>();

            List<Space> revitSpaces = coll.OfType<Space>().Cast<Space>().ToList();
            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions() { SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center };

            foreach (var revitSpace in revitSpaces )
            {
                if (revitSpace.Area <= 0.001) continue; // skip unenclosed spaces...

                var space = new Objects.Space() { Name = revitSpace.Name, Number = revitSpace.Number, UniqueId = revitSpace.UniqueId };

                Parameter airflow = revitSpace.get_Parameter(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM);
                space.Airflow = airflow.AsDouble() * 60.0; // conversion.
                spaces.Add(space);

                // now get all of the space boundary stuff.

                var segList =  revitSpace.GetBoundarySegments(options);

                // we just want the first (outer) list of segments.
                var outer = segList.First();

                XYZ lastPoint = null;
                foreach( var segment in outer )
                {
                    var curve = segment.GetCurve();
                    XYZ p1 = curve.GetEndPoint(0);
                    XYZ p2 = curve.GetEndPoint(1);
                    IList<XYZ> points = curve.Tessellate();

                    // make sure that we're taking the closer point, in case they're reversed.
                    if (lastPoint != null)
                    {
                        if (p2.DistanceTo(lastPoint) < p1.DistanceTo(lastPoint))
                        {
                            // we need to reverse stuff.
                            points = points.Reverse().ToList();

                            XYZ tmp = p1;
                            p1 = p2;
                            p2 = tmp;
                        }
                    }
                    lastPoint = p2;
                    foreach( var point in points.Skip(1) )
                    {
                        space.Points.Add(point);
                    }
                }
            }

            return spaces;
        }

        public IList<FamilyInstance> GetAllVAVs()
        {
            FilteredElementCollector coll = new FilteredElementCollector(_uiDoc.Document, _uiDoc.ActiveGraphicalView.Id);

            List<FamilyInstance> fis = new List<FamilyInstance>();

            foreach( var elem in coll.OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType().ToElements())
            {
                if (elem is FamilyInstance)
                {
                    // check that it's actually a VAV box, based on 
                    Element typeElem = _uiDoc.Document.GetElement(elem.GetTypeId());
                    Parameter sched = typeElem.GetParameters("SCHEDULE_TYPE").FirstOrDefault();
                    if ((sched != null) && (sched.AsString().ToUpper() == "AIR_TERMINAL_BOX"))
                    {
                        fis.Add(elem as FamilyInstance);
                    }
                }
            }

            return fis;
        }

        public IList<FamilyInstance> GetAllShaftLocations()
        {
            FilteredElementCollector coll = new FilteredElementCollector(_uiDoc.Document, _uiDoc.ActiveGraphicalView.Id);
            coll.OfCategory(BuiltInCategory.OST_GenericModel).WhereElementIsNotElementType();
            List<FamilyInstance> fis = new List<FamilyInstance>();

            foreach( var elem in coll.ToElements())
            {
                if (elem.Name.ToUpper() == "MECHANICAL SHAFT") fis.Add(elem as FamilyInstance);
            }

            return fis;
        }

        public IList<Line> GetAllCorridorLines()
        {
            FilteredElementCollector coll = new FilteredElementCollector(_uiDoc.Document, _uiDoc.ActiveGraphicalView.Id);
            var mlines = coll.OfClass(typeof(CurveElement)).WhereElementIsNotElementType().OfType<CurveElement>().Cast<ModelCurve>();

            List<Line> lines = new List<Line>();
            foreach(var mline in mlines)
            {
                // check the subcategory.
                if (mline.LineStyle.Name.ToUpper() == "DUCTWORK")
                {
                    if (mline.GeometryCurve is Line) lines.Add(mline.GeometryCurve as Line);
                }
                
            }

            return lines;
            
        }
        #endregion

        #region PrivateMethods

        private Objects.Node lookupExisting( XYZ point, IList<Objects.Node> nodes)
        {
            double tolerance = 0.1;

            foreach( var node in nodes )
            {
                if (node.Location.DistanceTo(point) < tolerance) return node;
            }

            return null;
        }
        private XYZ getClosest(XYZ point, IList<Line> lines, bool includeEnds)
        {
            // preferably get a normal projection. if requested fallback to nearest edge.
            double nearest = 99999;
            bool foundOne = false;
            XYZ outputXYZ = null;
            foreach (var line in lines)
            {
                IntersectionResult res = line.Project(point);

                if (res != null)
                {
                    foundOne = true;
                    if (res.Distance < nearest)
                    {
                        nearest = res.Distance;
                        outputXYZ = res.XYZPoint;
                    }
                }
            }

            if (foundOne) return outputXYZ;

            // if we didn't find one, consider endpoints if we are allowed.
            if (includeEnds == false) return null;

            foreach( var line in lines )
            {
                XYZ p1 = line.GetEndPoint(0);
                XYZ p2 = line.GetEndPoint(1);

                if (p1.DistanceTo(point) < nearest)
                {
                    nearest = p1.DistanceTo(point);
                    outputXYZ = p1;
                }
                if (p2.DistanceTo(point) < nearest)
                {
                    nearest = p2.DistanceTo(point);
                    outputXYZ = p2;
                }
            }

            return outputXYZ;
        }
        #endregion


    }
}
