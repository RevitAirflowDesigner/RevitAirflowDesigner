using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

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

            var save = new PushButtonData("AirflowDesigner", "Airflow" + Environment.NewLine + "Designer", System.Reflection.Assembly.GetExecutingAssembly().Location, "AirflowDesigner.Command");
            save.ToolTip = "Analyze the model for airflow, help make decisions";
            save.LongDescription = "Look at possible shaft locations";
            save.LargeImage = getImage("AirflowDesigner.Icons.path_finder_32.png");
            save.Image = getImage("AirflowDesigner.Icons.path_finder_16.png");

            var load = new PushButtonData("LoadAirflowDesigner", "Load" + Environment.NewLine + "Results", System.Reflection.Assembly.GetExecutingAssembly().Location, "AirflowDesigner.LoadCommand");
            load.ToolTip = "Load, Display and Generate";
            load.LongDescription = "Load, display and generate layout based on Selected Options";
            load.LargeImage = getImage("AirflowDesigner.Icons.document_32.png");
            load.Image = getImage("AirflowDesigner.Icons.document_16.png");

            panel.AddItem(save);
            panel.AddItem(load);
            panel.AddSlideOut();

            var clear = new PushButtonData("ClearAirflow", "Clear", System.Reflection.Assembly.GetExecutingAssembly().Location, "AirflowDesigner.ClearCommand");
            clear.ToolTip = "Clear any AVF geometry previews";
            panel.AddItem(clear);
        }

        private System.Windows.Media.ImageSource getImage(string imageFile)
        {
            try
            {
                System.IO.Stream stream = this.GetType().Assembly.GetManifestResourceStream(imageFile);
                if (stream == null) return null;
                PngBitmapDecoder pngDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return pngDecoder.Frames[0];

            }
            catch
            {
                return null; // no image


            }
        }
    }
}
