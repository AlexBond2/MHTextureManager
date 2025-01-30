namespace MHTextureManager
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            textureToolStripMenuItem = new ToolStripMenuItem();
            importDDSToolStripMenuItem = new ToolStripMenuItem();
            exportDDSToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            totalTexturesStatusLabel = new ToolStripStatusLabel();
            totalTexturesStatus = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            statusFiltered = new ToolStripStatusLabel();
            splitContainer1 = new SplitContainer();
            panel2 = new Panel();
            manifestProgressBar = new ProgressBar();
            manifestTreeView = new TreeView();
            panel1 = new Panel();
            filterClear = new Button();
            label1 = new Label();
            filterBox = new TextBox();
            panel4 = new Panel();
            texturePanel = new Panel();
            textureView = new PictureBox();
            panel5 = new Panel();
            textureFileLabel = new Label();
            label5 = new Label();
            mipMapsLabel = new Label();
            label4 = new Label();
            textureNameLabel = new Label();
            label3 = new Label();
            textureGuidLabel = new Label();
            label2 = new Label();
            panel3 = new Panel();
            texturesTree = new TreeView();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            panel4.SuspendLayout();
            texturePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)textureView).BeginInit();
            panel5.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, textureToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(886, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(65, 20);
            fileToolStripMenuItem.Text = "Manifest";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(161, 22);
            openToolStripMenuItem.Text = "Open Manifest...";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // textureToolStripMenuItem
            // 
            textureToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { importDDSToolStripMenuItem, exportDDSToolStripMenuItem });
            textureToolStripMenuItem.Name = "textureToolStripMenuItem";
            textureToolStripMenuItem.Size = new Size(57, 20);
            textureToolStripMenuItem.Text = "Texture";
            // 
            // importDDSToolStripMenuItem
            // 
            importDDSToolStripMenuItem.Name = "importDDSToolStripMenuItem";
            importDDSToolStripMenuItem.Size = new Size(144, 22);
            importDDSToolStripMenuItem.Text = "Import DDS...";
            importDDSToolStripMenuItem.Click += importDDSToolStripMenuItem_Click;
            // 
            // exportDDSToolStripMenuItem
            // 
            exportDDSToolStripMenuItem.Name = "exportDDSToolStripMenuItem";
            exportDDSToolStripMenuItem.Size = new Size(144, 22);
            exportDDSToolStripMenuItem.Text = "Export DDS...";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { totalTexturesStatusLabel, totalTexturesStatus, toolStripStatusLabel1, statusFiltered });
            statusStrip1.Location = new Point(0, 652);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(886, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // totalTexturesStatusLabel
            // 
            totalTexturesStatusLabel.Name = "totalTexturesStatusLabel";
            totalTexturesStatusLabel.Size = new Size(80, 17);
            totalTexturesStatusLabel.Text = "Total textures:";
            // 
            // totalTexturesStatus
            // 
            totalTexturesStatus.AutoSize = false;
            totalTexturesStatus.Name = "totalTexturesStatus";
            totalTexturesStatus.Size = new Size(50, 17);
            totalTexturesStatus.Text = "0";
            totalTexturesStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(49, 17);
            toolStripStatusLabel1.Text = "Filtered:";
            // 
            // statusFiltered
            // 
            statusFiltered.AutoSize = false;
            statusFiltered.Name = "statusFiltered";
            statusFiltered.Size = new Size(50, 17);
            statusFiltered.Text = "0";
            statusFiltered.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(3, 24);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panel2);
            splitContainer1.Panel1.Controls.Add(panel1);
            splitContainer1.Panel1MinSize = 210;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(panel4);
            splitContainer1.Panel2.Controls.Add(panel3);
            splitContainer1.Panel2MinSize = 210;
            splitContainer1.Size = new Size(886, 628);
            splitContainer1.SplitterDistance = 245;
            splitContainer1.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.Controls.Add(manifestProgressBar);
            panel2.Controls.Add(manifestTreeView);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 32);
            panel2.Name = "panel2";
            panel2.Size = new Size(245, 596);
            panel2.TabIndex = 2;
            // 
            // manifestProgressBar
            // 
            manifestProgressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            manifestProgressBar.Location = new Point(8, 8);
            manifestProgressBar.Name = "manifestProgressBar";
            manifestProgressBar.Size = new Size(230, 23);
            manifestProgressBar.TabIndex = 1;
            // 
            // manifestTreeView
            // 
            manifestTreeView.Dock = DockStyle.Fill;
            manifestTreeView.Location = new Point(0, 0);
            manifestTreeView.Name = "manifestTreeView";
            manifestTreeView.Size = new Size(245, 596);
            manifestTreeView.TabIndex = 0;
            manifestTreeView.AfterSelect += manifestTreeView_AfterSelect;
            // 
            // panel1
            // 
            panel1.Controls.Add(filterClear);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(filterBox);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(245, 32);
            panel1.TabIndex = 1;
            // 
            // filterClear
            // 
            filterClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            filterClear.BackColor = Color.Green;
            filterClear.FlatAppearance.BorderSize = 0;
            filterClear.FlatStyle = FlatStyle.Flat;
            filterClear.Font = new Font("Segoe UI", 9F);
            filterClear.ForeColor = Color.White;
            filterClear.ImageAlign = ContentAlignment.MiddleLeft;
            filterClear.Location = new Point(216, 3);
            filterClear.Name = "filterClear";
            filterClear.Size = new Size(24, 23);
            filterClear.TabIndex = 2;
            filterClear.Text = "X";
            filterClear.UseVisualStyleBackColor = false;
            filterClear.Click += filterClear_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 6);
            label1.Name = "label1";
            label1.Size = new Size(33, 15);
            label1.TabIndex = 1;
            label1.Text = "Filter";
            // 
            // filterBox
            // 
            filterBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            filterBox.Location = new Point(45, 3);
            filterBox.Name = "filterBox";
            filterBox.Size = new Size(168, 23);
            filterBox.TabIndex = 0;
            filterBox.KeyDown += filterBox_KeyDown;
            // 
            // panel4
            // 
            panel4.Controls.Add(texturePanel);
            panel4.Controls.Add(panel5);
            panel4.Dock = DockStyle.Fill;
            panel4.Location = new Point(0, 0);
            panel4.Name = "panel4";
            panel4.Size = new Size(637, 378);
            panel4.TabIndex = 5;
            // 
            // panel6
            // 
            texturePanel.AutoScroll = true;
            texturePanel.BackColor = Color.Silver;
            texturePanel.Controls.Add(textureView);
            texturePanel.Dock = DockStyle.Fill;
            texturePanel.Location = new Point(0, 72);
            texturePanel.Name = "panel6";
            texturePanel.Size = new Size(637, 306);
            texturePanel.TabIndex = 2;
            texturePanel.Resize += texturePanel_Resize;
            // 
            // textureView
            // 
            textureView.BackColor = Color.LightGray;
            textureView.BackgroundImage = Properties.Resources.bk;
            textureView.Location = new Point(0, 0);
            textureView.Name = "textureView";
            textureView.Size = new Size(256, 256);
            textureView.SizeMode = PictureBoxSizeMode.AutoSize;
            textureView.TabIndex = 1;
            textureView.TabStop = false;
            // 
            // panel5
            // 
            panel5.Controls.Add(textureFileLabel);
            panel5.Controls.Add(label5);
            panel5.Controls.Add(mipMapsLabel);
            panel5.Controls.Add(label4);
            panel5.Controls.Add(textureNameLabel);
            panel5.Controls.Add(label3);
            panel5.Controls.Add(textureGuidLabel);
            panel5.Controls.Add(label2);
            panel5.Dock = DockStyle.Top;
            panel5.Location = new Point(0, 0);
            panel5.Name = "panel5";
            panel5.Size = new Size(637, 72);
            panel5.TabIndex = 0;
            // 
            // textureFileLabel
            // 
            textureFileLabel.AutoSize = true;
            textureFileLabel.Location = new Point(105, 37);
            textureFileLabel.Name = "textureFileLabel";
            textureFileLabel.Size = new Size(0, 15);
            textureFileLabel.TabIndex = 7;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(3, 37);
            label5.Name = "label5";
            label5.Size = new Size(105, 15);
            label5.TabIndex = 6;
            label5.Text = "Texture File Cache:";
            // 
            // mipMapsLabel
            // 
            mipMapsLabel.AutoSize = true;
            mipMapsLabel.Location = new Point(105, 52);
            mipMapsLabel.Name = "mipMapsLabel";
            mipMapsLabel.Size = new Size(0, 15);
            mipMapsLabel.TabIndex = 5;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(48, 52);
            label4.Name = "label4";
            label4.Size = new Size(60, 15);
            label4.TabIndex = 4;
            label4.Text = "MipMaps:";
            // 
            // textureNameLabel
            // 
            textureNameLabel.AutoSize = true;
            textureNameLabel.Location = new Point(105, 22);
            textureNameLabel.Name = "textureNameLabel";
            textureNameLabel.Size = new Size(0, 15);
            textureNameLabel.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(25, 22);
            label3.Name = "label3";
            label3.Size = new Size(83, 15);
            label3.TabIndex = 2;
            label3.Text = "Texture Name:";
            // 
            // textureGuidLabel
            // 
            textureGuidLabel.AutoSize = true;
            textureGuidLabel.Location = new Point(105, 7);
            textureGuidLabel.Name = "textureGuidLabel";
            textureGuidLabel.Size = new Size(0, 15);
            textureGuidLabel.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(73, 7);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 0;
            label2.Text = "Guid:";
            // 
            // panel3
            // 
            panel3.Controls.Add(texturesTree);
            panel3.Dock = DockStyle.Bottom;
            panel3.Location = new Point(0, 378);
            panel3.Name = "panel3";
            panel3.Size = new Size(637, 250);
            panel3.TabIndex = 4;
            // 
            // texturesTree
            // 
            texturesTree.Dock = DockStyle.Fill;
            texturesTree.Location = new Point(0, 0);
            texturesTree.Name = "texturesTree";
            texturesTree.Size = new Size(637, 250);
            texturesTree.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(886, 674);
            Controls.Add(splitContainer1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(480, 410);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MH Texture Manager";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel4.ResumeLayout(false);
            texturePanel.ResumeLayout(false);
            texturePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)textureView).EndInit();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            panel3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private StatusStrip statusStrip1;
        private SplitContainer splitContainer1;
        private Panel panel1;
        private Label label1;
        private TextBox filterBox;
        private Panel panel2;
        private TreeView manifestTreeView;
        private Panel panel4;
        private Panel panel3;
        private PictureBox textureView;
        private Panel panel5;
        private Button filterClear;
        private TreeView texturesTree;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ProgressBar manifestProgressBar;
        private Label textureGuidLabel;
        private Label label2;
        private Label textureNameLabel;
        private Label label3;
        private Label textureFileLabel;
        private Label label5;
        private Label mipMapsLabel;
        private Label label4;
        private ToolStripStatusLabel totalTexturesStatusLabel;
        private ToolStripStatusLabel totalTexturesStatus;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel statusFiltered;
        private ToolStripMenuItem textureToolStripMenuItem;
        private ToolStripMenuItem importDDSToolStripMenuItem;
        private ToolStripMenuItem exportDDSToolStripMenuItem;
        private Panel texturePanel;
    }
}
