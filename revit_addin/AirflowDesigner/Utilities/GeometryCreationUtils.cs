using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Utilities
{
    public static class GeometryCreationUtils
    {
        internal static Solid CreateSolidFromBox(Autodesk.Revit.ApplicationServices.Application app, BoundingBoxXYZ box)
        {
            // create a set of curves from the base of the box.

            // presumes an untransformed box.
            XYZ A1 = box.Min;
            XYZ A2 = new XYZ(box.Max.X, box.Min.Y, box.Min.Z);
            XYZ A3 = new XYZ(box.Max.X, box.Max.Y, box.Min.Z);
            XYZ A4 = new XYZ(box.Min.X, box.Max.Y, box.Min.Z);

            List<Curve> crvs = new List<Curve>();

            crvs.Add(Line.CreateBound(A1, A2));
            crvs.Add(Line.CreateBound(A2, A3));
            crvs.Add(Line.CreateBound(A3, A4));
            crvs.Add(Line.CreateBound(A4, A1));

            CurveLoop loop = CurveLoop.Create(crvs);
            List<CurveLoop> loops = new List<CurveLoop>() { loop };

            Solid s = GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, (box.Max.Z - box.Min.Z));

            return s;
        }

        public static Solid CreateCylinder(Autodesk.Revit.ApplicationServices.Application app, XYZ origin, XYZ vector, double radius, double height)
        {
            Plane p = Plane.CreateByNormalAndOrigin(vector, origin);
            // need to create this as two arcs rather than one!
            Curve circle1 = Arc.Create(p, radius, 0, Math.PI);
            Curve circle2 = Arc.Create(p, radius, Math.PI, Math.PI * 2.0);

            CurveLoop profile = CurveLoop.Create(new List<Curve>(new Curve[2] { circle1, circle2 }));
            List<CurveLoop> loops = new List<CurveLoop>(new CurveLoop[1] { profile });

            Solid cyl = GeometryCreationUtilities.CreateExtrusionGeometry(loops, vector, height);

            return cyl;
        }

    }
}
