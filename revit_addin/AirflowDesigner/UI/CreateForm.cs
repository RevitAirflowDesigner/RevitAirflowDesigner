using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirflowDesigner.UI
{
    public partial class CreateForm : Form
    {
        private Controllers.Controller _controller;
        private Objects.Solution _sol;
        private IList<Objects.Node> _nodes;

        public CreateForm(Controllers.Controller c, Objects.Solution sol, IList<Objects.Node> nodes)
        {
            InitializeComponent();
            _controller = c;
            _nodes = nodes;
            _sol = sol;

            var ductSystems = Controllers.MEPController.GetDuctSystemTypes(c.GetDocument());
            cbSystem.Items.AddRange(ductSystems.ToArray());
            cbDuctTypes.Items.AddRange(Controllers.MEPController.GetDuctTypes(c.GetDocument(), Controllers.MEPController.DuctShapeEnum.Round).ToArray());


            cbSystem.SelectedItem = ductSystems.FirstOrDefault(s => s.Name.ToUpper().Contains("SUPPLY"));
            cbDuctTypes.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                var sys = cbSystem.SelectedItem as Autodesk.Revit.DB.Mechanical.MechanicalSystemType;
                var typ = cbDuctTypes.SelectedItem as Autodesk.Revit.DB.Mechanical.DuctType;

                _controller.DrawSolution(_sol, _nodes, sys.Id, typ.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().Name + ": " + ex.Message);
            }
        }
    }
}
