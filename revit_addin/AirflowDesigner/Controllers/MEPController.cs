using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;

namespace AirflowDesigner.Controllers
{
    public static class MEPController
    {
        public enum DuctShapeEnum { Round, Rectangular, Oval };

        public static MechanicalSystemType[] GetDuctSystemTypes(Document doc)
        {
            FilteredElementCollector coll = new FilteredElementCollector(doc);
            coll.OfClass(typeof(MechanicalSystemType));

            return coll.Cast<MechanicalSystemType>().ToArray();
        }

        public static DuctType[] GetDuctTypes(Document doc, DuctShapeEnum shape)
        {
            FilteredElementCollector coll = new FilteredElementCollector(doc);
            coll.OfClass(typeof(DuctType));

            IEnumerable<DuctType> pts = coll.Cast<DuctType>();

            if ((pts == null) || (pts.Count<DuctType>() == 0)) return null;

            List<string> familyNames = new List<string>();

            switch (shape)
            {
                case DuctShapeEnum.Round:
                    familyNames.Add("Round Duct");
                    familyNames.Add("Gaine circulaire");
                    break;

                case DuctShapeEnum.Rectangular:
                    familyNames.Add("Rectangular Duct");
                    familyNames.Add("Gaine rectangulaire");
                    break;

                case DuctShapeEnum.Oval:
                    familyNames.Add("Oval Duct");
                    familyNames.Add("Gaine ovale");
                    break;
            }

            List<DuctType> types = new List<DuctType>();
            foreach (DuctType dt in pts)
            {

                Parameter p = dt.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM);
                foreach (string familyName in familyNames)
                {
                    if (p.AsString().ToUpper() == familyName.ToUpper())
                    {
                        types.Add(dt);
                    }
                }
            }
            if (types.Count == 0) return null;
            return types.ToArray();

        }
        public static Duct MakeDuct(Document doc, XYZ p1, XYZ p2, ElementId typ, ElementId mst, object diameter, object insulationThick)
        {
            if (typ == null) throw new ArgumentNullException("A Duct Type must be specified");

          

            // use default level (actually - it appears that this doesn't work???
            ElementId level = new ElementId(-1);
            IList<Level> levelsBelow = getAllLevelsBelow(doc, Math.Min(p1.Z, p2.Z));
            Level levElement = null;
            if (levelsBelow != null)
                levElement = levelsBelow[0];
            else
                levElement = getClosestLevel(doc, Math.Min(p1.Z, p2.Z));
            Duct d = Duct.Create(doc, mst, typ, levElement.Id, p1, p2);

            if (diameter != null)
            {
                if (diameter is String)
                {
                    d.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).SetValueString(diameter.ToString());
                }
                if (diameter is double)
                {

                    d.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).Set(Convert.ToDouble(diameter));
                }
            }
            if (insulationThick != null)
            {
                //if (insulationThick is String)
                //{
                //    d.get_Parameter(BuiltInParameter.RBS_PIPE_INSULATION_THICKNESS).SetValueString(insulationThick.ToString());
                //}
                //if (insulationThick is double)
                //{
                //    double ins = Convert.ToDouble(insulationThick);
                //    if (ins != 0) d.get_Parameter(BuiltInParameter.RBS_PIPE_INSULATION_THICKNESS).Set(Convert.ToDouble(insulationThick));
                //}
                double thick = Convert.ToDouble(insulationThick);
                if (thick > 0)
                {
                    DuctInsulation test = DuctInsulation.Create(doc, d.Id, new ElementId(-1), Convert.ToDouble(insulationThick));
                }
            }


            return d;
        }

        public static Connector GetProperConnector(FamilyInstance fi, FlowDirectionType dir, DuctSystemType dst)
        {
            if (fi.MEPModel == null) return null;

            foreach( Connector conn in fi.MEPModel.ConnectorManager.Connectors)
            {
                if ((conn.Direction == dir) && (conn.DuctSystemType == dst)) return conn;
            }

            return null;
        }

        public static Connector GetNearestConnector(MEPCurve crv, XYZ point)
        {
            double nearest = 999999;
            Connector nearC = null;
            foreach( Connector c in crv.ConnectorManager.Connectors)
            {
                double dist = c.Origin.DistanceTo(point);
                if (dist < nearest)
                {
                    nearest = dist;
                    nearC = c;
                }
            }

            return nearC;
        }

        public static IList<FamilyInstance> JoinDucts(IList<Duct> ducts)
        {
            // we want to go through the ducts.
            // find ducts that have coincident endpoints.
            List<FamilyInstance> fittings = new List<FamilyInstance>();

            foreach (var duct in ducts)
            {
                foreach (Connector conn in duct.ConnectorManager.UnusedConnectors)
                {
                    foreach (var other in ducts)
                    {
                        if (other.Id == duct.Id) continue; // not the same.

                        foreach (Connector otherConn in other.ConnectorManager.UnusedConnectors)
                        {
                            double dist = otherConn.Origin.DistanceTo(conn.Origin);
                            if (dist < 0.01)
                            {
                                // what's the angle between them.
                                double angle = conn.CoordinateSystem.BasisZ.AngleTo(otherConn.CoordinateSystem.BasisZ);

                                // see what kind.
                                if ((angle < 0.001) || (angle > 0.99 * Math.PI))
                                {
                                    // straight!
                                    FamilyInstance fi = 
                                        duct.Document.Create.NewTransitionFitting(conn, otherConn);
                                    if (fi != null) fittings.Add(fi);
                                }
                                else
                                {
                                    // elbow
                                    FamilyInstance fi =
                                        duct.Document.Create.NewElbowFitting(conn, otherConn);
                                    if (fi != null) fittings.Add(fi);
                                }

                            }
                        }

                    }
                }
            }
            return fittings;
        }

        public static FamilyInstance MakeTakeOff(Connector c, MEPCurve crv)
        {
            return crv.Document.Create.NewTakeoffFitting(c, crv);
        }

        public static void MoveFittingAway(FamilyInstance fi, double distance, out MEPCurve moved)
        {
            // move the fitting from the larger side towards the smaller side by a 
            // certain distance, so that we can more easily T into it.
            moved = null;

            Transaction t = null;
            if (fi.Document.IsModifiable == false)
            {
                t = new Transaction(fi.Document, "Move Fitting");
                t.Start();
            }

            // determine which way to move.
            Connector c1 = null;
            Connector c2 = null;
            getOppositeConnectors(fi, out c1, out c2);

            // which way is bigger?
            XYZ dir = null;
            MEPCurve other = null;
            MEPCurve bigger = null;
            if (c1.Radius > c2.Radius)
            {
                // move towards c2;
                dir = c2.Origin.Subtract(c1.Origin).Normalize();

                other = getConnectedCurve(c2);
                bigger = getConnectedCurve(c1);
                
            }
            else
            {
                // move towards c1;
                dir = c1.Origin.Subtract(c2.Origin).Normalize();
                other = getConnectedCurve(c1);
                bigger = getConnectedCurve(c2);
            }
            
            // sanity check that the connector we're moving towards, that the connected MEPCurve 
            // actually is long enough to move that far!
            if ((other != null))
            {
                LocationCurve lc = other.Location as LocationCurve;
                if (lc != null)
                {
                    if (lc.Curve.Length < distance)
                    {
                        other.Document.Application.WriteJournalComment("NOTE: Unable to move fitting by " + distance + " because it is only " + lc.Curve.Length + " long.", false);
                        return;
                    }
                }
            }


            ElementTransformUtils.MoveElement(fi.Document, fi.Id, dir.Multiply(distance));

            fi.Document.Regenerate();
            moved = bigger;
            if (t != null)
            {
                t.Commit();
            }

        }

        #region PrivateMethods

        private static MEPCurve getConnectedCurve( Connector c )
        {
            if (c.IsConnected == false) return null;

            foreach( Connector other in c.AllRefs)
            {
                MEPCurve crv = other.Owner as MEPCurve;
                if (crv != null) return crv;
            }

            return null;
        }

        private static void getOppositeConnectors(FamilyInstance fi, out Connector c1, out Connector c2)
        {
            // for these connectors, there's basically almost always two
            List<Connector> conns = new List<Connector>();
            foreach( Connector c in fi.MEPModel.ConnectorManager.Connectors)
            {
                conns.Add(c);
            }

            c1 = conns.First();
            c2 = conns.Last();
        }
        private static IList<Level> getAllLevelsBelow(Document doc, double elevation)
        {
            FilteredElementCollector coll = new FilteredElementCollector(doc);
            coll.OfClass(typeof(Level));

            IList<Level> all = coll.ToElements().Cast<Level>().ToList();

            IEnumerable<Level> below = all.Where(level => level.ProjectElevation <= elevation).OrderByDescending(lev => lev.ProjectElevation);
            List<Level> levels = below.ToList<Level>();
            if ((levels == null) || (levels.Count == 0)) return null;

            return levels;
        }

        private static Level getClosestLevel(Document doc, double elevation)
        {
            FilteredElementCollector coll = new FilteredElementCollector(doc);

            coll.OfClass(typeof(Level));

            Level closest = null;
            double maxDist = 99999;

            IEnumerable<Level> levels = coll.Cast<Level>();

            foreach (Level lev in levels)
            {
                double dist = Math.Abs(lev.ProjectElevation - elevation);

                if (dist < maxDist)
                {
                    maxDist = dist;
                    closest = lev;
                }
            }

            return closest;
        }

        #endregion
    }
}
