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

        public Load(Controllers.Controller c, Autodesk.Revit.UI.UIApplication uiApp)
        {
            InitializeComponent();
            _controller = c;
            //_controller.View = this;
            _uiApp = uiApp;

            //register for idling callback
            _uiApp.Idling += _uiApp_Idling;
            _action = ActionEnum.None;

        }
       
        private void _uiApp_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            ActionEnum tmp = _action;
            _action = ActionEnum.None;

            switch (tmp)
            {
               
                   

               
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

                    //_controller.DeSerialize(fileName);

                    MessageBox.Show("The ductwork layout results has been loaded.");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().Name + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
            }

        }
    }
}
