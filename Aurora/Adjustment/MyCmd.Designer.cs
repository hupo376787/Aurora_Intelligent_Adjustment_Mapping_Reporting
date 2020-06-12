namespace Adjustment
{
    partial class MyCmd
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MyCmd));
            this.label1 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Cut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Clear = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Enter = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Name = "label1";
            this.label1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label1_MouseDown);
            this.label1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label1_MouseMove);
            this.label1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label1_MouseUp);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.Black;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.ContextMenuStrip = this.contextMenuStrip1;
            resources.ApplyResources(this.richTextBox1, "richTextBox1");
            this.richTextBox1.ForeColor = System.Drawing.Color.Silver;
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox1_KeyDown);
            this.richTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBox1_KeyPress);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Copy,
            this.toolStripMenuItem_Cut,
            this.toolStripMenuItem_Paste,
            this.toolStripMenuItem_Clear,
            this.toolStripMenuItem_Enter});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            // 
            // toolStripMenuItem_Copy
            // 
            this.toolStripMenuItem_Copy.Image = global::Adjustment.Properties.Resources.CmdCopy_16;
            this.toolStripMenuItem_Copy.Name = "toolStripMenuItem_Copy";
            resources.ApplyResources(this.toolStripMenuItem_Copy, "toolStripMenuItem_Copy");
            this.toolStripMenuItem_Copy.Click += new System.EventHandler(this.toolStripMenuItem_Copy_Click);
            // 
            // toolStripMenuItem_Cut
            // 
            this.toolStripMenuItem_Cut.Image = global::Adjustment.Properties.Resources.CmdCut_16;
            this.toolStripMenuItem_Cut.Name = "toolStripMenuItem_Cut";
            resources.ApplyResources(this.toolStripMenuItem_Cut, "toolStripMenuItem_Cut");
            this.toolStripMenuItem_Cut.Click += new System.EventHandler(this.toolStripMenuItem_Cut_Click);
            // 
            // toolStripMenuItem_Paste
            // 
            this.toolStripMenuItem_Paste.Image = global::Adjustment.Properties.Resources.CmdPaste_16;
            this.toolStripMenuItem_Paste.Name = "toolStripMenuItem_Paste";
            resources.ApplyResources(this.toolStripMenuItem_Paste, "toolStripMenuItem_Paste");
            this.toolStripMenuItem_Paste.Click += new System.EventHandler(this.toolStripMenuItem_Paste_Click);
            // 
            // toolStripMenuItem_Clear
            // 
            this.toolStripMenuItem_Clear.Image = global::Adjustment.Properties.Resources.CmdClear_16;
            this.toolStripMenuItem_Clear.Name = "toolStripMenuItem_Clear";
            resources.ApplyResources(this.toolStripMenuItem_Clear, "toolStripMenuItem_Clear");
            this.toolStripMenuItem_Clear.Click += new System.EventHandler(this.toolStripMenuItem_Clear_Click);
            // 
            // toolStripMenuItem_Enter
            // 
            this.toolStripMenuItem_Enter.Image = global::Adjustment.Properties.Resources.CmdEnter_16;
            this.toolStripMenuItem_Enter.Name = "toolStripMenuItem_Enter";
            resources.ApplyResources(this.toolStripMenuItem_Enter, "toolStripMenuItem_Enter");
            this.toolStripMenuItem_Enter.Click += new System.EventHandler(this.toolStripMenuItem_Enter_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::Adjustment.Properties.Resources.MyCmd_24;
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Adjustment.Properties.Resources.Close;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            this.pictureBox1.MouseHover += new System.EventHandler(this.pictureBox1_MouseHover);
            // 
            // MyCmd
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "MyCmd";
            this.ShowInTaskbar = false;
            this.Activated += new System.EventHandler(this.MyCmd_Activated);
            this.Load += new System.EventHandler(this.MyCmd_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Copy;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Paste;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Cut;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Enter;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Clear;

    }
}