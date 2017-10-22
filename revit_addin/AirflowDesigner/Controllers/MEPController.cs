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

        public static void JoinDucts(IList<Duct> ducts)
        {
            // we want to go through the ducts.
            // find ducts that have coincident endpoints.

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
                                conn.ConnectTo(otherConn);
                            }
                        }

                    }
                }
            }
        }

        #region PrivateMethods
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
