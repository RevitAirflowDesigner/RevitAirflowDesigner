using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;

namespace AirflowDesigner
{
    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    public class LoadCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {

                Load f = new Load();

                f.ShowDialog();

                return Result.Succeeded;
            }
            catch (ApplicationException aex)
            {
                MessageBox.Show(aex.Message);
            }
            catch (Exception ex)
            {
                TaskDialog td = new TaskDialog("Error");
                td.MainContent = "An unexpected error occurred: " + Environment.NewLine + ex.GetType().Name + ": " + ex.Message;
                td.ExpandedContent = ex.StackTrace;

                td.Show();
            }

            return Result.Failed;
        }
    }
}
