using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace AirflowDesigner
{
    public class ExternalApp : IExternalApplication
    {
        private Autodesk.Revit.UI.UIControlledApplication _app;

        public Result OnShutdown(UIControlledApplication application)
        {
          
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            _app = application;
            buildUI();
            return Result.Succeeded;
        }

        private void buildUI()
        {
            var panel = _app.CreateRibbonPanel(Tab.AddIns, "Airflow" + Environment.NewLine + "Designer");

            var save = new PushButtonData("AirflowDesigner", "Airflow Designer", System.Reflection.Assembly.GetExecutingAssembly().Location, "AirflowDesigner.Command");
            save.ToolTip = "Analyze the model for airflow, help make decisions";
            save.LongDescription = "Look at possible shaft locations";
            //save.LargeImage = getImage("LifeSaver.Images.lifesaver-32.png");
            //save.Image = getImage("LifeSaver.Images.lifesaver-16.png");

            var load = new PushButtonData("AirflowDesigner", "Airflow Designer", System.Reflection.Assembly.GetExecutingAssembly().Location, "AirflowDesigner.Load");
            load.ToolTip = "Load, Display and Generate";
            load.LongDescription = "Load, display and generate layout based on Selected Options";

            panel.AddItem(save);
            panel.AddItem(load);
        }
    }
}
