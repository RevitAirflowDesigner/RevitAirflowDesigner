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

                // temp.
                Objects.Results res = new Objects.Results();
                res.Nodes.Add(new Objects.Node() { Name = "Node1V", Location = new XYZ(0, 0, 0), NodeType = Objects.Node.NodeTypeEnum.Vav, SpaceId = "1234" });
                res.Nodes.Add(new Objects.Node() { Name = "Node2C", Location = new XYZ(0, 0, 0), NodeType = Objects.Node.NodeTypeEnum.Other, SpaceId = null });
                res.Nodes.Add(new Objects.Node() { Name = "Node3C", Location = new XYZ(0, 0, 0), NodeType = Objects.Node.NodeTypeEnum.Other, SpaceId = null });
                res.Nodes.Add(new Objects.Node() { Name = "Node4S", Location = new XYZ(0, 0, 0), NodeType = Objects.Node.NodeTypeEnum.Shaft, SpaceId = "1234" });

                Objects.Solution sol = new Objects.Solution() { Shaft = "Shaft1", Cost = 10, SheetMetal = 5, StaticPressure = 5 };
                sol.Edges.Add(new Objects.Edge() { Node1 = 1, Node2 = 2, Airflow = 5, Diameter = 5, Distance = 10 });
                sol.Edges.Add(new Objects.Edge() { Node1 = 2, Node2 = 3, Airflow = 7, Diameter = 6, Distance = 2 });
                sol.Edges.Add(new Objects.Edge() { Node1 = 3, Node2 = 4, Airflow = 9, Distance = 5, Diameter = 2 });

                res.Solutions.Add(sol);

                sol = new Objects.Solution() { Shaft = "Shaft1", Cost = 10, SheetMetal = 5, StaticPressure = 5 };
                sol.Edges.Add(new Objects.Edge() { Node1 = 3, Node2 = 1, Airflow = 4, Diameter = 10, Distance = 11 });
                sol.Edges.Add(new Objects.Edge() { Node1 = 1, Node2 = 2, Airflow = 8, Diameter = 7, Distance = 8 });
                sol.Edges.Add(new Objects.Edge() { Node1 = 2, Node2 = 4, Airflow = 12, Distance = 15, Diameter = 9 });
                res.Solutions.Add(sol);

                string results = Newtonsoft.Json.JsonConvert.SerializeObject(res);

                System.IO.File.WriteAllText(@"C:\Temp\Sample-results.json", results);



                Controllers.Controller c = new Controllers.Controller(commandData.Application.ActiveUIDocument);

                UI.Load f = new UI.Load(c, commandData.Application);

                f.Show();

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
