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

            List<Objects.Node> nodes = new List<Objects.Node>();
            List<Objects.Edge> edges = new List<Objects.Edge>();

            // figure out the current phase.
            Phase phase = _uiDoc.Document.GetElement(_uiDoc.ActiveGraphicalView.get_Parameter(BuiltInParameter.VIEW_PHASE).AsElementId()) as Phase;

            foreach( var vav in VAVs)
            {
                XYZ location = (vav.Location as LocationPoint).Point;

                Objects.Node n = new Objects.Node() { Location = location, Name = "VAV-" + vav.Id.IntegerValue, NodeType = Objects.Node.NodeTypeEnum.Vav };

                // determine the related space.
                var relatedSpace = vav.get_Space(phase);
                if (relatedSpace != null) n.SpaceId = relatedSpace.UniqueId;

            }

            // project onto nearest corridor line, and make an edge.

            return new Objects.Network();

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
                if (elem.Name.ToUpper() == "SHAFT") fis.Add(elem as FamilyInstance);
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
        //private XYZ getClosest( XYZ point, IList<Line> lines)
        //{
        //    double nearest = 99999;
        //    foreach( var line in lines )
        //    {
        //        IntersectionResult res = line.Project(point);

        //        if (res != null)
        //        {
        //            nearest = res.
        //        }
        //    }
        //}
        #endregion


    }
}
