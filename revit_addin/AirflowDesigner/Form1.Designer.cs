namespace AirflowDesigner
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.llVAV = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.llSpaces = new System.Windows.Forms.LinkLabel();
            this.llShafts = new System.Windows.Forms.LinkLabel();
            this.btnGo = new System.Windows.Forms.Button();
            this.llCorridors = new System.Windows.Forms.LinkLabel();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(13, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(348, 49);
            this.label1.TabIndex = 0;
            this.label1.Text = "This app will assist you with evaluating options for shaft location and routing o" +
    "f MEP ductwork. The routing is currently from VAV box locations to a single shaf" +
    "t.\r\n";
            // 
            // llVAV
            // 
            this.llVAV.AutoSize = true;
            this.llVAV.Location = new System.Drawing.Point(37, 147);
            this.llVAV.Name = "llVAV";
            this.llVAV.Size = new System.Drawing.Size(114, 13);
            this.llVAV.TabIndex = 1;
            this.llVAV.TabStop = true;
            this.llVAV.Text = "Number of VAV boxes:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Current Model:";
            // 
            // llSpaces
            // 
            this.llSpaces.AutoSize = true;
            this.llSpaces.Location = new System.Drawing.Point(37, 122);
            this.llSpaces.Name = "llSpaces";
            this.llSpaces.Size = new System.Drawing.Size(98, 13);
            this.llSpaces.TabIndex = 3;
            this.llSpaces.TabStop = true;
            this.llSpaces.Text = "Number of Spaces:";
            // 
            // llShafts
            // 
            this.llShafts.AutoSize = true;
            this.llShafts.Location = new System.Drawing.Point(37, 176);
            this.llShafts.Name = "llShafts";
            this.llShafts.Size = new System.Drawing.Size(184, 13);
            this.llShafts.TabIndex = 4;
            this.llShafts.TabStop = true;
            this.llShafts.Text = "Number of Proposed Shaft Locations:";
            // 
            // btnGo
            // 
            this.btnGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGo.Location = new System.Drawing.Point(286, 229);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 23);
            this.btnGo.TabIndex = 5;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // llCorridors
            // 
            this.llCorridors.AutoSize = true;
            this.llCorridors.Location = new System.Drawing.Point(37, 203);
            this.llCorridors.Name = "llCorridors";
            this.llCorridors.Size = new System.Drawing.Size(126, 13);
            this.llCorridors.TabIndex = 6;
            this.llCorridors.TabStop = true;
            this.llCorridors.Text = "Number of Corridor Lines:";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "*.json|JSON Files(*.json)|*.*|All Files";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 296);
            this.Controls.Add(this.llCorridors);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.llShafts);
            this.Controls.Add(this.llSpaces);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.llVAV);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Airflow Designer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel llVAV;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel llSpaces;
        private System.Windows.Forms.LinkLabel llShafts;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.LinkLabel llCorridors;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}