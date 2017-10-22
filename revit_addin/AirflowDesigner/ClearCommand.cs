using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace AirflowDesigner
{
    [Transaction(TransactionMode.Manual)]
    public class ClearCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Utilities.AVFUtility.Clear(commandData.Application.ActiveUIDocument);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error: " + ex.GetType().Name, ex.Message);
            }

            return Result.Succeeded;

        }
    }
}
