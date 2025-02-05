namespace MHTextureManager
{
    partial class SettingsForm
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
            radioNew = new RadioButton();
            groupBox1 = new GroupBox();
            selectOpen = new Button();
            createOpen = new Button();
            replaceBox = new TextBox();
            addBox = new TextBox();
            createBox = new TextBox();
            radioAdd = new RadioButton();
            radioReplace = new RadioButton();
            importButton = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // radioNew
            // 
            radioNew.AutoSize = true;
            radioNew.Checked = true;
            radioNew.Location = new Point(19, 24);
            radioNew.Name = "radioNew";
            radioNew.Size = new Size(49, 19);
            radioNew.TabIndex = 0;
            radioNew.TabStop = true;
            radioNew.Text = "New";
            radioNew.UseVisualStyleBackColor = true;
            radioNew.CheckedChanged += radioNew_CheckedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(selectOpen);
            groupBox1.Controls.Add(createOpen);
            groupBox1.Controls.Add(replaceBox);
            groupBox1.Controls.Add(addBox);
            groupBox1.Controls.Add(createBox);
            groupBox1.Controls.Add(radioAdd);
            groupBox1.Controls.Add(radioReplace);
            groupBox1.Controls.Add(radioNew);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(540, 115);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Import in TFC";
            // 
            // selectOpen
            // 
            selectOpen.Enabled = false;
            selectOpen.Location = new Point(451, 51);
            selectOpen.Name = "selectOpen";
            selectOpen.Size = new Size(75, 23);
            selectOpen.TabIndex = 7;
            selectOpen.Text = "Select...";
            selectOpen.UseVisualStyleBackColor = true;
            selectOpen.Click += selectOpen_Click;
            // 
            // createOpen
            // 
            createOpen.Location = new Point(451, 22);
            createOpen.Name = "createOpen";
            createOpen.Size = new Size(75, 23);
            createOpen.TabIndex = 6;
            createOpen.Text = "Save as...";
            createOpen.UseVisualStyleBackColor = true;
            createOpen.Click += createOpen_Click;
            // 
            // replaceBox
            // 
            replaceBox.Enabled = false;
            replaceBox.Location = new Point(91, 80);
            replaceBox.Name = "replaceBox";
            replaceBox.Size = new Size(435, 23);
            replaceBox.TabIndex = 5;
            // 
            // addBox
            // 
            addBox.Enabled = false;
            addBox.Location = new Point(91, 51);
            addBox.Name = "addBox";
            addBox.Size = new Size(354, 23);
            addBox.TabIndex = 4;
            // 
            // createBox
            // 
            createBox.Enabled = false;
            createBox.Location = new Point(91, 22);
            createBox.Name = "createBox";
            createBox.Size = new Size(354, 23);
            createBox.TabIndex = 3;
            // 
            // radioAdd
            // 
            radioAdd.AutoSize = true;
            radioAdd.Location = new Point(19, 53);
            radioAdd.Name = "radioAdd";
            radioAdd.Size = new Size(47, 19);
            radioAdd.TabIndex = 1;
            radioAdd.Text = "Add";
            radioAdd.UseVisualStyleBackColor = true;
            radioAdd.CheckedChanged += radioAdd_CheckedChanged;
            // 
            // radioReplace
            // 
            radioReplace.AutoSize = true;
            radioReplace.Location = new Point(19, 82);
            radioReplace.Name = "radioReplace";
            radioReplace.Size = new Size(66, 19);
            radioReplace.TabIndex = 2;
            radioReplace.Text = "Replace";
            radioReplace.UseVisualStyleBackColor = true;
            radioReplace.CheckedChanged += radioReplace_CheckedChanged;
            // 
            // importButton
            // 
            importButton.DialogResult = DialogResult.OK;
            importButton.Location = new Point(463, 133);
            importButton.Name = "importButton";
            importButton.Size = new Size(75, 23);
            importButton.TabIndex = 3;
            importButton.Text = "Import";
            importButton.UseVisualStyleBackColor = true;
            importButton.Click += importButton_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(564, 162);
            Controls.Add(importButton);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Import Texture Settings";
            Load += SettingsForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private RadioButton radioNew;
        private GroupBox groupBox1;
        private RadioButton radioAdd;
        private RadioButton radioReplace;
        private Button importButton;
        private TextBox replaceBox;
        private TextBox addBox;
        private TextBox createBox;
        private Button selectOpen;
        private Button createOpen;
    }
}