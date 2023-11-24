namespace Beep.CSVManager.Winform
{
    partial class uc_csvmanager
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
            this.components = new System.ComponentModel.Container();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.fileDefinitionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.columnsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.wellsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.GetFolderbutton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.fileDefinitionBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wellsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.DataSource = this.columnsBindingSource;
            this.listBox1.DisplayMember = "Value";
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(48, 222);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(137, 485);
            this.listBox1.TabIndex = 0;
            this.listBox1.ValueMember = "Value";
            // 
            // listBox2
            // 
            this.listBox2.DataSource = this.wellsBindingSource;
            this.listBox2.DisplayMember = "Value";
            this.listBox2.FormattingEnabled = true;
            this.listBox2.Location = new System.Drawing.Point(448, 222);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(137, 485);
            this.listBox2.TabIndex = 1;
            this.listBox2.ValueMember = "Value";
            // 
            // fileDefinitionBindingSource
            // 
            this.fileDefinitionBindingSource.DataSource = typeof(Beep.CSVManager.FileDefinition);
            // 
            // columnsBindingSource
            // 
            this.columnsBindingSource.DataMember = "Columns";
            this.columnsBindingSource.DataSource = this.fileDefinitionBindingSource;
            // 
            // wellsBindingSource
            // 
            this.wellsBindingSource.DataMember = "Wells";
            this.wellsBindingSource.DataSource = this.fileDefinitionBindingSource;
            // 
            // GetFolderbutton
            // 
            this.GetFolderbutton.Location = new System.Drawing.Point(510, 110);
            this.GetFolderbutton.Name = "GetFolderbutton";
            this.GetFolderbutton.Size = new System.Drawing.Size(75, 23);
            this.GetFolderbutton.TabIndex = 2;
            this.GetFolderbutton.Text = "Get Folder";
            this.GetFolderbutton.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(48, 139);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(537, 20);
            this.textBox1.TabIndex = 3;
            // 
            // uc_csvmanager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.GetFolderbutton);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.listBox1);
            this.Name = "uc_csvmanager";
            this.Size = new System.Drawing.Size(890, 763);
            ((System.ComponentModel.ISupportInitialize)(this.fileDefinitionBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wellsBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.BindingSource fileDefinitionBindingSource;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.BindingSource columnsBindingSource;
        private System.Windows.Forms.BindingSource wellsBindingSource;
        private System.Windows.Forms.Button GetFolderbutton;
        private System.Windows.Forms.TextBox textBox1;
    }
}
