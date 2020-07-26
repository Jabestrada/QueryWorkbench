namespace QueryWorkbenchUI.UserControls {
    partial class QueryWorkspaceView {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.txtConnString = new System.Windows.Forms.TextBox();
            this.queryAndParametersContainer = new System.Windows.Forms.SplitContainer();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.txtParams = new System.Windows.Forms.TextBox();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.resultsTab = new System.Windows.Forms.TabControl();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.queryAndParametersContainer)).BeginInit();
            this.queryAndParametersContainer.Panel1.SuspendLayout();
            this.queryAndParametersContainer.Panel2.SuspendLayout();
            this.queryAndParametersContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtConnString
            // 
            this.txtConnString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConnString.Location = new System.Drawing.Point(62, 15);
            this.txtConnString.Name = "txtConnString";
            this.txtConnString.Size = new System.Drawing.Size(564, 20);
            this.txtConnString.TabIndex = 0;
            this.txtConnString.Text = "Database=qwb_test;Server=.;Trusted_Connection=True;";
            this.txtConnString.TextChanged += new System.EventHandler(this.txtConnString_TextChanged);
            // 
            // queryAndParametersContainer
            // 
            this.queryAndParametersContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.queryAndParametersContainer.Location = new System.Drawing.Point(0, 0);
            this.queryAndParametersContainer.Name = "queryAndParametersContainer";
            // 
            // queryAndParametersContainer.Panel1
            // 
            this.queryAndParametersContainer.Panel1.Controls.Add(this.txtQuery);
            // 
            // queryAndParametersContainer.Panel2
            // 
            this.queryAndParametersContainer.Panel2.Controls.Add(this.txtParams);
            this.queryAndParametersContainer.Size = new System.Drawing.Size(626, 144);
            this.queryAndParametersContainer.SplitterDistance = 520;
            this.queryAndParametersContainer.TabIndex = 4;
            // 
            // txtQuery
            // 
            this.txtQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtQuery.Location = new System.Drawing.Point(0, 0);
            this.txtQuery.Multiline = true;
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtQuery.Size = new System.Drawing.Size(520, 144);
            this.txtQuery.TabIndex = 0;
            this.txtQuery.Text = "SELECT * FROM Person\r\nSELECT * FROM Company";
            this.txtQuery.TextChanged += new System.EventHandler(this.txtQuery_TextChanged);
            // 
            // txtParams
            // 
            this.txtParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtParams.Location = new System.Drawing.Point(0, 0);
            this.txtParams.Multiline = true;
            this.txtParams.Name = "txtParams";
            this.txtParams.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtParams.Size = new System.Drawing.Size(102, 144);
            this.txtParams.TabIndex = 0;
            this.txtParams.TextChanged += new System.EventHandler(this.txtParams_TextChanged);
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainSplitContainer.Location = new System.Drawing.Point(3, 41);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.queryAndParametersContainer);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.resultsTab);
            this.mainSplitContainer.Size = new System.Drawing.Size(626, 291);
            this.mainSplitContainer.SplitterDistance = 144;
            this.mainSplitContainer.TabIndex = 5;
            // 
            // resultsTab
            // 
            this.resultsTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultsTab.Location = new System.Drawing.Point(0, 0);
            this.resultsTab.Name = "resultsTab";
            this.resultsTab.SelectedIndex = 0;
            this.resultsTab.Size = new System.Drawing.Size(626, 143);
            this.resultsTab.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Conn string";
            // 
            // QueryWorkspaceView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.txtConnString);
            this.Name = "QueryWorkspaceView";
            this.Size = new System.Drawing.Size(629, 335);
            this.queryAndParametersContainer.Panel1.ResumeLayout(false);
            this.queryAndParametersContainer.Panel1.PerformLayout();
            this.queryAndParametersContainer.Panel2.ResumeLayout(false);
            this.queryAndParametersContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.queryAndParametersContainer)).EndInit();
            this.queryAndParametersContainer.ResumeLayout(false);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtConnString;
        private System.Windows.Forms.SplitContainer queryAndParametersContainer;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.TextBox txtParams;
        private System.Windows.Forms.TabControl resultsTab;
        private System.Windows.Forms.Label label1;
    }
}
