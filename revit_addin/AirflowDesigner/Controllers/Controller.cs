using Autodesk.Revit.Creation;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using AirflowDesigner.UI;
using System.IO;

namespace AirflowDesigner.Controllers
{
    public class Controller
    {
        #region Declarations
        private UIDocument _uiDoc;
        private double _ductWorkElevation = 0;
        #endregion

        #region Constructor
        public Controller( UIDocument doc)
        {
            _uiDoc = doc;
        }
        #endregion

        #region PublicMethods

        public Objects.Network BuildNetwork( IList<Objects.Space> spaces, IList<FamilyInstance> VAVs, IList<FamilyInstance> shafts, IList<Line> corridorLines, bool biDirectional = false )
        {
            // here we want to start with a node for every VAV, linked to the appropriate space.
            // then project the VAV onto the corridor lines.
            // then build the edges...

            Objects.Network network = new Objects.Network();
            List<Objects.Node> nodes = new List<Objects.Node>();
            List<Objects.Edge> edges = new List<Objects.Edge>();
            network.Edges = edges;
            network.Nodes = nodes;
            network.Spaces = spaces.ToList();

           

            _ductWorkElevation = (VAVs.First().Location as LocationPoint).Point.Z;
            corridorLines = resetLines(corridorLines);


            // figure out the current phase.
            Phase phase = _uiDoc.Document.GetElement(_uiDoc.ActiveGraphicalView.get_Parameter(BuiltInParameter.VIEW_PHASE).AsElementId()) as Phase;

            foreach( var vav in VAVs)
            {
                XYZ location = (vav.Location as LocationPoint).Point;
                Connector c = MEPController.GetProperConnector(vav, FlowDirectionType.In, DuctSystemType.SupplyAir);
                if (c != null) location = c.Origin;
                location = normalizeZ(location);

                Objects.Node n = new Objects.Node() { Location = location, Name = "VAV-" + vav.Id.IntegerValue, NodeType = Objects.Node.NodeTypeEnum.Vav };
               

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

                // adding this later.
                nodes.Add(n);

                // make an edge from VAV to corridor.
                Objects.Edge edge = new Objects.Edge() { Node1 = n.Id, Node2 = connNode.Id, Distance = n.Location.DistanceTo(connNode.Location) };
                log("Made edge from " + n.Name + " to " + connNode.Name);
                edges.Add(edge);

            }

            // now let's do the same thing with the shaft.
            foreach( var shaft in shafts)
            {
                XYZ location = (shaft.Location as LocationPoint).Point;
                location = normalizeZ(location);

                // shaft name is based on the mark
                Parameter mark = shaft.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                string name = (String.IsNullOrEmpty(mark.AsString()) ? shaft.Id.IntegerValue.ToString() : mark.AsString());
                Objects.Node n = new Objects.Node() { Location = location, Name = "Shaft-" + name, NodeType = Objects.Node.NodeTypeEnum.Shaft };

              
                // now we need to find where this connects to the 

                //CURRENT SIMPLIFICATION: NO SHAFT WILL BE ON TOP OF A CENTERLINE.
                // COME BACK AND FIX THIS  LATER!

                XYZ connection = getClosest(location, corridorLines, true);

                Objects.Node connNode = lookupExisting(connection, nodes);
                if (connNode == null)
                {
                    // make a new node
                    connNode = new Objects.Node() { NodeType = Objects.Node.NodeTypeEnum.Other, Name = "Corridor-To-Shaft", Location = connection };
                    nodes.Add(connNode);
                }
                // add this later.
                nodes.Add(n);

                // make an edge that connects
                Objects.Edge edge = new Objects.Edge() { Node1 = connNode.Id, Node2 = n.Id, Distance = (connNode.Location.DistanceTo(n.Location)) };
                log("Made edge from " + connNode.Name + " to " + n.Name);
                edges.Add(edge);
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
                    log("Made corridor edge from " + onLine[i - 1].Name + " to " + onLine[i].Name);
                    edges.Add(corrEdge);

                    if (biDirectional)
                    {
                        Objects.Edge c2 = new Objects.Edge() { Node2 = onLine[i - 1].Id, Node1 = onLine[i].Id, Distance = onLine[i - 1].Location.DistanceTo(onLine[i].Location) };
                        log("Made corridor edge from " + onLine[i].Name + " to " + onLine[i-1].Name);
                        edges.Add(c2);
                    }
                }

            }



            return network;

        }

        private IList<Line> resetLines(IList<Line> inLines)
        {
            List<Line> lines = new List<Line>();

            foreach( var line in inLines)
            {
                XYZ p1 = line.GetEndPoint(0);
                XYZ p2 = line.GetEndPoint(1);
                var newLn = Line.CreateBound(new XYZ(p1.X, p1.Y, _ductWorkElevation), new XYZ(p2.X, p2.Y, _ductWorkElevation));
                lines.Add(newLn);
            }

            return lines;
        }

        public void Serialize(Objects.Network nw, string filename)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(nw);

            System.IO.File.WriteAllText(filename, json);

        }

        public Objects.Results DeSerialize(string filename)
        {
            string json = System.IO.File.ReadAllText(filename);

            Objects.Results res = Newtonsoft.Json.JsonConvert.DeserializeObject<Objects.Results>(json,
                                    new Newtonsoft.Json.JsonConverter[] { new Objects.XYZDeserializer()});

            return res;
        }

    
        public string getDefaultPrefix()
        {
            // we want it to be based on the location of the file.

            if (String.IsNullOrEmpty(_uiDoc.Document.PathName))
            {
                return null;
            }

            return _uiDoc.Document.PathName.Replace(".rvt", "");
        }

        public void DrawSolution(Objects.Solution sol, IList<Objects.Node> nodes, ElementId system, ElementId ductType)
        {
            Transaction t = null;
            if(_uiDoc.Document.IsModifiable == false)
            {
                t = new Transaction(_uiDoc.Document, "Create Ductwork");
                t.Start();
            }

            Utilities.AVFUtility.Clear(_uiDoc);


            // start with the corridor
            IList<Objects.Edge> corrEdges = sol.GetCorridorEdges(nodes);

            List<Duct> corrDucts = new List<Duct>();
            List<Duct> allDucts = new List<Duct>();
            SubTransaction st = new SubTransaction(_uiDoc.Document);
            st.Start();
            foreach( var edge in corrEdges )
            {
                Objects.Node n1 = nodes.Single(n => n.Id == edge.Node1);
                Objects.Node n2 = nodes.Single(n => n.Id == edge.Node2);

                Duct d = 
                    MEPController.MakeDuct(_uiDoc.Document, n1.Location, n2.Location, ductType, system, edge.Diameter, 0.0);

                corrDucts.Add(d);
                allDucts.Add(d);

            }
            st.Commit();
            _uiDoc.Document.Regenerate();

            IList<FamilyInstance> fittings = MEPController.JoinDucts(corrDucts);

            IList<Objects.Edge> vavEdges = sol.GetVAVEdges(nodes);

            IList<MEPCurve> crvDucts = corrDucts.Cast<MEPCurve>().ToList();

            foreach ( var edge in vavEdges)
            {
                //Objects.Node n1 = nodes.Single(n => n.Id == edge.Node1);
                //Objects.Node n2 = nodes.Single(n => n.Id == edge.Node2);

                //MEPController.MakeDuct(_uiDoc.Document, n1.Location, n2.Location, ductType, system, edge.Diameter, 0.0);

                Duct d = createVAVConnection(edge, ductType, system, nodes, crvDucts, fittings);


            }

            IList<Objects.Edge> shaftEdges = sol.GetShaftEdges(nodes);

            foreach( var edge in shaftEdges )
            {
                Objects.Node n1 = nodes.Single(n => n.Id == edge.Node1);
                Objects.Node n2 = nodes.Single(n => n.Id == edge.Node2);

                MEPController.MakeDuct(_uiDoc.Document, n1.Location, n2.Location, ductType, system, edge.Diameter, 0.0);

            }

            if (t != null) t.Commit();
        }
        public Autodesk.Revit.DB.Document GetDocument() { return _uiDoc.Document; }

        public void ShowSolution(Objects.Solution sol, IList<Objects.Node> nodes, string colorBy)
        {
            Utilities.AVFUtility.Clear(_uiDoc);

            List<Solid> solids = new List<Solid>();
            List<Double> values = new List<double>();
            foreach( var edge in sol.Edges )
            {
                Objects.Node n1 = nodes.Single(n => n.Id == edge.Node1);
                Objects.Node n2 = nodes.Single(n => n.Id == edge.Node2);

                var cyl = Utilities.GeometryCreationUtils.CreateCylinder(_uiDoc.Application.Application, n1.Location, n2.Location.Subtract(n1.Location).Normalize(), edge.Diameter / 2.0, n1.Location.DistanceTo(n2.Location));
                solids.Add(cyl);

                double val = 0;
                switch (colorBy)
                {
                    case "Diameter":
                        val = edge.Diameter;
                        break;
                    case "Airflow":
                        val = edge.Airflow;
                        break;
                }
                values.Add(val);

            }

            Utilities.AVFUtility.ShowSolids(_uiDoc.Document, solids, values);
        }

        public void DrawNetwork(Objects.Network nw)
        {
            Transaction t = null;
            if (_uiDoc.Document.IsModifiable == false)
            {
                t = new Transaction(_uiDoc.Document, "DrawNetwork");
                t.Start();
            }

            // draw all nodes and edges.

            foreach( var node in nw.Nodes)
            {
                // draw a circle...
                // Utilities.GeometryCreationUtils.DrawCircle(_uiDoc.Document, Plane.CreateByNormalAndOrigin( XYZ.BasisZ, node.Location), _uiDoc.ActiveGraphicalView.SketchPlane, 1.0);
            }

            List<Line> lines = new List<Line>();
            foreach( var edge in nw.Edges)
            {
                Objects.Node n1 = nw.Nodes.Single(n => n.Id == edge.Node1);
                Objects.Node n2 = nw.Nodes.Single(n => n.Id == edge.Node2);


                double dist = n1.Location.DistanceTo(n2.Location);
                if (dist < _uiDoc.Application.Application.ShortCurveTolerance)
                {
                    string stop = "";
                }
                else
                {
                    Line line = Line.CreateBound(n1.Location, n2.Location);
                    lines.Add(line);
                }

            }
            Utilities.GeometryCreationUtils.DrawLines(_uiDoc.Document, lines);

            if (t != null) t.Commit();
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
        private Duct createVAVConnection(Objects.Edge edge, ElementId ductType, ElementId system, IList<Objects.Node> nodes, IList<MEPCurve> curves, IList<FamilyInstance> fittings)
        {
            Objects.Node n1 = nodes.Single(n => n.Id == edge.Node1);
            Objects.Node n2 = nodes.Single(n => n.Id == edge.Node2);

            Objects.Node vavNode = n1;
            if (n1.NodeType != Objects.Node.NodeTypeEnum.Vav) vavNode = n2;

            Objects.Node corrNode = n1;
            if (n1.NodeType != Objects.Node.NodeTypeEnum.Other) corrNode = n2;

            // find the nearest VAV to vavNode;

            // determine if we need to shift the connector on the corridor
            var fi = isFittingAtPoint(corrNode.Location, fittings, 0.1);

            MEPCurve toConnect = null;
            if (fi != null)
            {
                MEPController.MoveFittingAway(fi, edge.Diameter, out toConnect);
            }

            Duct d =
                MEPController.MakeDuct(_uiDoc.Document, vavNode.Location, corrNode.Location, ductType, system, edge.Diameter, 0.0);

            Connector tap = MEPController.GetNearestConnector(d, corrNode.Location);
           
            if (toConnect == null)
            {
                toConnect = findNearestCurve(corrNode.Location, curves, 0.05);
            }

            FamilyInstance fi2 = MEPController.MakeTakeOff(tap, toConnect);
            if (fi2 != null) fittings.Add(fi2);

            return d;


        }

        private FamilyInstance isFittingAtPoint(XYZ pt, IList<FamilyInstance> fis, double tolerance)
        {
            foreach( FamilyInstance fi in fis )
            {
                if (fi.MEPModel == null) continue;
                foreach( Connector c in fi.MEPModel.ConnectorManager.Connectors)
                {
                    double dist = c.Origin.DistanceTo(pt);
                    if (dist < tolerance) return fi;
                }
            }

            return null;
        }

        private MEPCurve findNearestCurve(XYZ pt, IList<MEPCurve> curves, double tolerance)
        {
            double nearest = 9999999;
            MEPCurve nearestCrv = null;
            foreach (MEPCurve crv in curves)
            {
                LocationCurve lc = crv.Location as LocationCurve;
                if (lc != null)
                {
                    var result = lc.Curve.Project(pt);
                    if ((result != null) && (result.Distance < tolerance))
                    {
                        if (result.Distance < nearest)
                        {
                            nearest = result.Distance;
                            nearestCrv = crv;
                        }
                    }
                }
            }

            return nearestCrv;
        }


        private IList<Objects.Node> getNodesOnLine(Line cl, IList<Objects.Node> nodes)
        {
            // find all of the nodes that are on the given line.
            List<Objects.Node> onLine = new List<Objects.Node>();

            foreach (var node in nodes)
            {
                
                var result = cl.Project(node.Location);
                if ((result != null) && (result.Distance < 0.001)) onLine.Add(node);
            }

            return onLine;
        }

        private void log(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
            _uiDoc.Application.Application.WriteJournalComment(msg, false);
        }
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

            if (foundOne) return normalizeZ( outputXYZ);

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

            return normalizeZ(outputXYZ);
        }

        private XYZ normalizeZ(XYZ point)
        {
            return new XYZ(point.X, point.Y, _ductWorkElevation);
        }
        #endregion


    }
}
