using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace AirflowDesigner.Objects
{
    public class Space
    {
        public String Name { get; set; }
        public String Number { get; set; }
        public double Airflow { get; set; }

        public List<XYZ> Points;
    }

}
