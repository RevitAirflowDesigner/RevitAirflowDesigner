using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
                var network = _controller.BuildNetwork(_spaces, _vavs, _shafts, _lines);

                _controller.DrawNetwork(network);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().Name + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
