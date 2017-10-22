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
    public partial class Load : System.Windows.Forms.Form
    {
        private Controllers.Controller _controller;
        private Autodesk.Revit.UI.UIApplication _uiApp;
        private enum ActionEnum { None, Show, DrawRoute };
        private ActionEnum _action = ActionEnum.None;
        private Objects.Results _results;
        private int _lastSortCol = 1;
        private ListSortDirection _lastDir;

        public Load(Controllers.Controller c, Autodesk.Revit.UI.UIApplication uiApp, string filename = null)
        {
            InitializeComponent();
            _controller = c;
            //_controller.View = this;
            _uiApp = uiApp;

            //register for idling callback
            _uiApp.Idling += _uiApp_Idling;
            _action = ActionEnum.None;

            btn_Generate.Enabled = false;
            cbColorBy.Items.Add("Diameter");
            cbColorBy.Items.Add("Airflow");
            cbColorBy.SelectedIndex = 0;

            try
            {
                if (filename != null) loadResults(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loading Issue: " + ex.GetType().Name + ": " + ex.Message);
            }
            

        }
       
        private void _uiApp_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            ActionEnum tmp = _action;
            _action = ActionEnum.None;

            switch (tmp)
            {

                case ActionEnum.Show:
                    performShow();
                    break;

                case ActionEnum.DrawRoute:
                    performCreate();
                    break;

               
            }

        }

       
        private void performCreate()
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    Objects.Solution sol = dataGridView1.SelectedRows[0].DataBoundItem as Objects.Solution;
                    if (sol != null)
                    {

                        UI.CreateForm create = new CreateForm(_controller, sol, _results.Nodes);
                        if (create.ShowDialog(this) == DialogResult.OK)
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btn_Load_Click(object sender, EventArgs e)
        {
            try
            {

                string fileName = openFileDialog1.FileName;

                // prompt the user for the output file.
                if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                {

                    loadResults(openFileDialog1.FileName);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().Name + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
            }

        }

        private void loadResults(string filename)
        {
            _results = _controller.DeSerialize(filename);

            // hoops to make it sortable
            UI.SortableBindingList<Objects.Solution> solutions = new UI.SortableBindingList<Objects.Solution>(_results.Solutions);

            dataGridView1.DataSource = solutions;
            dataGridView1.Update();
        }

        private void performShow()
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    Objects.Solution sol = dataGridView1.SelectedRows[0].DataBoundItem as Objects.Solution;
                    if (sol != null)
                    {
                        _controller.ShowSolution(sol, _results.Nodes, cbColorBy.SelectedItem.ToString());
                    }
                    btn_Generate.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private void performGenerate()
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    Objects.Solution sol = dataGridView1.SelectedRows[0].DataBoundItem as Objects.Solution;
                    if (sol != null)
                    {
                        string colorBy = cbColorBy.SelectedItem.ToString();

                        _controller.ShowSolution(sol, _results.Nodes, colorBy);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    
                }
                else
                {
                    MessageBox.Show("Please select a row to generate.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private void onRowSelected(object sender, EventArgs e)
        {
            _action = ActionEnum.Show;
        }

        private void btn_Generate_Click(object sender, EventArgs e)
        {
            _action = ActionEnum.DrawRoute;
        }

        private void onCellHeaderClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            ListSortDirection dir = ListSortDirection.Ascending;

            if ((_lastSortCol == -1)||(_lastSortCol != e.ColumnIndex))
            {
                dir = ListSortDirection.Ascending;
                dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            }
            else
            {
                dir = ListSortDirection.Descending;
                dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = SortOrder.Descending;
            }


            dataGridView1.Sort(dataGridView1.Columns[e.ColumnIndex], dir);
            _lastSortCol = e.ColumnIndex;

        }
    }
}
