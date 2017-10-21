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
        #region Declarations
        public String Name { get; set; }
        public String Number { get; set; }
        public double Airflow { get; set; }

        public String UniqueId { get; set; }

        public List<XYZ> Points;
        #endregion

        #region Constructor
        public Space()
        {
            Points = new List<XYZ>();
        }
        #endregion

    }

}
