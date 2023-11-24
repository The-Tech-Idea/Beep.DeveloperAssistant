namespace Beep.DeveloperAssistant.WinformCore
{
    partial class uc_CodeConverter
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TopsplitContainer = new SplitContainer();
            ToEntitybutton = new Button();
            label1 = new Label();
            label2 = new Label();
            splitContainer1 = new SplitContainer();
            SourcetextBox = new TextBox();
            TargettextBox = new TextBox();
            ((System.ComponentModel.ISupportInitialize)TopsplitContainer).BeginInit();
            TopsplitContainer.Panel1.SuspendLayout();
            TopsplitContainer.Panel2.SuspendLayout();
            TopsplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // TopsplitContainer
            // 
            TopsplitContainer.BorderStyle = BorderStyle.FixedSingle;
            TopsplitContainer.Dock = DockStyle.Fill;
            TopsplitContainer.FixedPanel = FixedPanel.Panel1;
            TopsplitContainer.IsSplitterFixed = true;
            TopsplitContainer.Location = new Point(0, 0);
            TopsplitContainer.Margin = new Padding(4, 3, 4, 3);
            TopsplitContainer.Name = "TopsplitContainer";
            TopsplitContainer.Orientation = Orientation.Horizontal;
            // 
            // TopsplitContainer.Panel1
            // 
            TopsplitContainer.Panel1.Controls.Add(ToEntitybutton);
            TopsplitContainer.Panel1.Controls.Add(label1);
            TopsplitContainer.Panel1.Controls.Add(label2);
            // 
            // TopsplitContainer.Panel2
            // 
            TopsplitContainer.Panel2.Controls.Add(splitContainer1);
            TopsplitContainer.Size = new Size(1189, 931);
            TopsplitContainer.SplitterDistance = 72;
            TopsplitContainer.SplitterWidth = 5;
            TopsplitContainer.TabIndex = 0;
            // 
            // ToEntitybutton
            // 
            ToEntitybutton.Anchor = AnchorStyles.Top;
            ToEntitybutton.Location = new Point(496, 35);
            ToEntitybutton.Margin = new Padding(4, 3, 4, 3);
            ToEntitybutton.Name = "ToEntitybutton";
            ToEntitybutton.Size = new Size(155, 27);
            ToEntitybutton.TabIndex = 2;
            ToEntitybutton.Text = "Class To Entity";
            ToEntitybutton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(3, 35);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(86, 25);
            label1.TabIndex = 0;
            label1.Text = "Source";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(1061, 33);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(80, 25);
            label2.TabIndex = 1;
            label2.Text = "Target";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(SourcetextBox);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(TargettextBox);
            splitContainer1.Size = new Size(1187, 852);
            splitContainer1.SplitterDistance = 583;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // SourcetextBox
            // 
            SourcetextBox.Dock = DockStyle.Fill;
            SourcetextBox.Location = new Point(0, 0);
            SourcetextBox.Margin = new Padding(4, 3, 4, 3);
            SourcetextBox.Multiline = true;
            SourcetextBox.Name = "SourcetextBox";
            SourcetextBox.ScrollBars = ScrollBars.Both;
            SourcetextBox.Size = new Size(583, 852);
            SourcetextBox.TabIndex = 0;
            // 
            // TargettextBox
            // 
            TargettextBox.Dock = DockStyle.Fill;
            TargettextBox.Location = new Point(0, 0);
            TargettextBox.Margin = new Padding(4, 3, 4, 3);
            TargettextBox.Multiline = true;
            TargettextBox.Name = "TargettextBox";
            TargettextBox.ScrollBars = ScrollBars.Both;
            TargettextBox.Size = new Size(599, 852);
            TargettextBox.TabIndex = 1;
            // 
            // uc_CodeConverter
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(TopsplitContainer);
            Margin = new Padding(4, 3, 4, 3);
            Name = "uc_CodeConverter";
            Size = new Size(1189, 931);
            TopsplitContainer.Panel1.ResumeLayout(false);
            TopsplitContainer.Panel1.PerformLayout();
            TopsplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)TopsplitContainer).EndInit();
            TopsplitContainer.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer TopsplitContainer;
        private SplitContainer splitContainer1;
        private TextBox SourcetextBox;
        private TextBox TargettextBox;
        private Button ToEntitybutton;
        private Label label2;
        private Label label1;
    }
}
