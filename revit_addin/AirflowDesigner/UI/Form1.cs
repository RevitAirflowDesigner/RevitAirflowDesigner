using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirflowDesigner
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private Controllers.Controller _controller;
        private IList<FamilyInstance> _vavs;
        private IList<FamilyInstance> _shafts;
        private IList<Objects.Space> _spaces;
        private IList<Line> _lines;
        

        public Form1(Controllers.Controller c)
        {
            InitializeComponent();

            _controller = c;
            _spaces = c.GetAllSpaces();
            _vavs = c.GetAllVAVs();
            _shafts = c.GetAllShaftLocations();
            _lines = c.GetAllCorridorLines();

            renderForm();
        }

        private void renderForm()
        {

            llSpaces.Text = "Number of Spaces: " + _spaces.Count;
            llVAV.Text = "Number of VAV boxes: " + _vavs.Count;
            llShafts.Text = "Number of Shaft Locations: " + _shafts.Count;
            llCorridors.Text = "Number of Corridor Lines: " + _lines.Count;
            validate();
        }

        private void validate()
        {
            bool isReady = true;
            if (_spaces.Count == 0) isReady = false;
            if (_vavs.Count == 0) isReady = false;
            if (_shafts.Count == 0) isReady = false;
            if (_lines.Count ==0) isReady = false;

            btnGo.Enabled = isReady;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {

                saveFileDialog1.FileName = getDefaultName();

                // prompt the user for the output file.
                if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    var network = _controller.BuildNetwork(_spaces, _vavs, _shafts, _lines);

                    _controller.DrawNetwork(network);

                    _controller.Serialize(network, saveFileDialog1.FileName);

                    MessageBox.Show("The duct network has been saved." +Environment.NewLine + 
                                    "  # of Nodes: " + network.Nodes.Count + Environment.NewLine + 
                                    "  # of Edges: " + network.Edges.Count + Environment.NewLine + Environment.NewLine + 
                                    "Launching Analysis");

                    var results = _controller.Calculate(saveFileDialog1.FileName);

                    if (results.Error == false)
                    {
                        // launch the results form.
                        launchResults(results);

                    }
                    else
                    {
                        MessageBox.Show("An error occurred while analyzing the data: " + results.ErrorMessage);
                    }

                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }

                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().Name + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void launchResults(Objects.AnalysisResults res)
        {
            UI.Load f = new UI.Load(_controller, _controller.GetUIDoc().Application, res.File );
            f.Text += " (Runtime: " + res.Span + ")";

            IntPtr currentRevitWin = Utilities.WindowsUtils.GetMainWindowHandle();
            if (currentRevitWin != null)
            {
                UI.WindowHandle handle = new UI.WindowHandle(currentRevitWin);

                f.Show(handle);
                Utilities.WindowsUtils.RegisterModeless(f);  // keep track of it, so that we can close it later and prevent two at the same time.
            }
            else
            {
                f.Show();
            }
        }

        private string getDefaultName()
        {
            string prefix = _controller.getDefaultPrefix();
            if (String.IsNullOrEmpty(prefix) == false)
            {
                prefix = prefix + ".json";
                return prefix;
            }

            return String.Empty;
        }
    }
}
