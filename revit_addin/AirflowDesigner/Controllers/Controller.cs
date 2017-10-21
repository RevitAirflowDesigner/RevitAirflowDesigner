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
            _doc = doc;
        }
        #endregion

        #region PublicMethods
        public IList<Objects.Space> GetAllSpaces()
        {
            // get representations of all spaces in the current view.
            FilteredElementCollector coll = new FilteredElementCollector(_uiDoc.Document, _uiDoc.ActiveGraphicalView.Id);

            coll.OfCategory(BuiltInCategory.OST_MEPSpaces);

            List<Objects.Space> spaces = new List<Objects.Space>();

            List<Space> revitSpaces = coll.OfType<Space>().Cast<Space>().ToList();

            foreach (var revitSpace in revitSpaces )
            {
                var space = new Objects.Space() {  }
            }
        }
        #endregion

        #region PrivateMethods

        #endregion


    }
}
