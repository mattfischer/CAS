namespace CAS
{
    partial class MainWindow
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.OutputPanel = new System.Windows.Forms.Panel();
            this.CommandBox = new System.Windows.Forms.TextBox();
            this.showIntermediates = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.OutputPanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.showIntermediates);
            this.splitContainer1.Panel2.Controls.Add(this.CommandBox);
            this.splitContainer1.Size = new System.Drawing.Size(292, 298);
            this.splitContainer1.SplitterDistance = 215;
            this.splitContainer1.TabIndex = 0;
            // 
            // OutputPanel
            // 
            this.OutputPanel.AutoScroll = true;
            this.OutputPanel.BackColor = System.Drawing.Color.White;
            this.OutputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutputPanel.Location = new System.Drawing.Point(0, 0);
            this.OutputPanel.Name = "OutputPanel";
            this.OutputPanel.Size = new System.Drawing.Size(292, 215);
            this.OutputPanel.TabIndex = 0;
            this.OutputPanel.SizeChanged += new System.EventHandler(this.OutputWindow_SizeChanged);
            this.OutputPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.OutputWindow_Paint);
            this.OutputPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OutputWindow_MouseClick);
            // 
            // CommandBox
            // 
            this.CommandBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.CommandBox.Location = new System.Drawing.Point(0, 0);
            this.CommandBox.Multiline = true;
            this.CommandBox.Name = "CommandBox";
            this.CommandBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CommandBox.Size = new System.Drawing.Size(292, 56);
            this.CommandBox.TabIndex = 0;
            this.CommandBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CommandBox_KeyDown);
            // 
            // showIntermediates
            // 
            this.showIntermediates.AutoSize = true;
            this.showIntermediates.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.showIntermediates.Location = new System.Drawing.Point(0, 62);
            this.showIntermediates.Name = "showIntermediates";
            this.showIntermediates.Size = new System.Drawing.Size(292, 17);
            this.showIntermediates.TabIndex = 1;
            this.showIntermediates.Text = "Show intermediate steps";
            this.showIntermediates.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 298);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainWindow";
            this.Text = "CAS Window";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel OutputPanel;
        private System.Windows.Forms.TextBox CommandBox;
        private System.Windows.Forms.CheckBox showIntermediates;


    }
}

