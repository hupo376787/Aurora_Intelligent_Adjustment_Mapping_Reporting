namespace Adjustment
{
    partial class AboutAurora
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutAurora));
            this.waterTime = new System.Windows.Forms.Timer(this.components);
            this.dropsTime = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Dynamic = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_ReadMind = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Weibo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Website = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_DxDiag = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Perfmon = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Resmon = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Msinfo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Winver = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.ScrollText_Timer1 = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // waterTime
            // 
            this.waterTime.Interval = 15;
            this.waterTime.Tick += new System.EventHandler(this.waterTime_Tick);
            // 
            // dropsTime
            // 
            this.dropsTime.Interval = 50000;
            this.dropsTime.Tick += new System.EventHandler(this.dropsTime_Tick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Exit,
            this.toolStripMenuItem_Dynamic,
            this.toolStripMenuItem_ReadMind,
            this.toolStripMenuItem_Weibo,
            this.toolStripMenuItem_Website,
            this.toolStripSeparator1,
            this.toolStripMenuItem_DxDiag,
            this.toolStripMenuItem_Perfmon,
            this.toolStripMenuItem_Resmon,
            this.toolStripMenuItem_Msinfo,
            this.toolStripMenuItem_Winver});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            // 
            // toolStripMenuItem_Exit
            // 
            this.toolStripMenuItem_Exit.Image = global::Adjustment.Properties.Resources.ExitAbout_24;
            this.toolStripMenuItem_Exit.Name = "toolStripMenuItem_Exit";
            resources.ApplyResources(this.toolStripMenuItem_Exit, "toolStripMenuItem_Exit");
            this.toolStripMenuItem_Exit.Click += new System.EventHandler(this.toolStripMenuItem_Exit_Click);
            // 
            // toolStripMenuItem_Dynamic
            // 
            this.toolStripMenuItem_Dynamic.Checked = true;
            this.toolStripMenuItem_Dynamic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem_Dynamic.Name = "toolStripMenuItem_Dynamic";
            resources.ApplyResources(this.toolStripMenuItem_Dynamic, "toolStripMenuItem_Dynamic");
            this.toolStripMenuItem_Dynamic.Click += new System.EventHandler(this.toolStripMenuItem_Dynamic_Click);
            // 
            // toolStripMenuItem_ReadMind
            // 
            this.toolStripMenuItem_ReadMind.Image = global::Adjustment.Properties.Resources.ReadMind_24;
            this.toolStripMenuItem_ReadMind.Name = "toolStripMenuItem_ReadMind";
            resources.ApplyResources(this.toolStripMenuItem_ReadMind, "toolStripMenuItem_ReadMind");
            this.toolStripMenuItem_ReadMind.Click += new System.EventHandler(this.toolStripMenuItem_ReadMind_Click);
            // 
            // toolStripMenuItem_Weibo
            // 
            this.toolStripMenuItem_Weibo.Image = global::Adjustment.Properties.Resources.Weibo_24;
            this.toolStripMenuItem_Weibo.Name = "toolStripMenuItem_Weibo";
            resources.ApplyResources(this.toolStripMenuItem_Weibo, "toolStripMenuItem_Weibo");
            this.toolStripMenuItem_Weibo.Click += new System.EventHandler(this.toolStripMenuItem_Weibo_Click);
            // 
            // toolStripMenuItem_Website
            // 
            this.toolStripMenuItem_Website.Image = global::Adjustment.Properties.Resources.Iexplore_24;
            this.toolStripMenuItem_Website.Name = "toolStripMenuItem_Website";
            resources.ApplyResources(this.toolStripMenuItem_Website, "toolStripMenuItem_Website");
            this.toolStripMenuItem_Website.Click += new System.EventHandler(this.toolStripMenuItem_Website_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // toolStripMenuItem_DxDiag
            // 
            this.toolStripMenuItem_DxDiag.Image = global::Adjustment.Properties.Resources.Dxdiag_24;
            this.toolStripMenuItem_DxDiag.Name = "toolStripMenuItem_DxDiag";
            resources.ApplyResources(this.toolStripMenuItem_DxDiag, "toolStripMenuItem_DxDiag");
            this.toolStripMenuItem_DxDiag.Click += new System.EventHandler(this.toolStripMenuItem_DxDiag_Click);
            // 
            // toolStripMenuItem_Perfmon
            // 
            this.toolStripMenuItem_Perfmon.Image = global::Adjustment.Properties.Resources.Perfmon_24;
            this.toolStripMenuItem_Perfmon.Name = "toolStripMenuItem_Perfmon";
            resources.ApplyResources(this.toolStripMenuItem_Perfmon, "toolStripMenuItem_Perfmon");
            this.toolStripMenuItem_Perfmon.Click += new System.EventHandler(this.toolStripMenuItem_Perfmon_Click);
            // 
            // toolStripMenuItem_Resmon
            // 
            this.toolStripMenuItem_Resmon.Image = global::Adjustment.Properties.Resources.Resmon_24;
            this.toolStripMenuItem_Resmon.Name = "toolStripMenuItem_Resmon";
            resources.ApplyResources(this.toolStripMenuItem_Resmon, "toolStripMenuItem_Resmon");
            this.toolStripMenuItem_Resmon.Click += new System.EventHandler(this.toolStripMenuItem_Resmon_Click);
            // 
            // toolStripMenuItem_Msinfo
            // 
            this.toolStripMenuItem_Msinfo.Image = global::Adjustment.Properties.Resources.Msinfo_24;
            this.toolStripMenuItem_Msinfo.Name = "toolStripMenuItem_Msinfo";
            resources.ApplyResources(this.toolStripMenuItem_Msinfo, "toolStripMenuItem_Msinfo");
            this.toolStripMenuItem_Msinfo.Click += new System.EventHandler(this.toolStripMenuItem_Msinfo_Click);
            // 
            // toolStripMenuItem_Winver
            // 
            this.toolStripMenuItem_Winver.Image = global::Adjustment.Properties.Resources.Winver_24;
            this.toolStripMenuItem_Winver.Name = "toolStripMenuItem_Winver";
            resources.ApplyResources(this.toolStripMenuItem_Winver, "toolStripMenuItem_Winver");
            this.toolStripMenuItem_Winver.Click += new System.EventHandler(this.toolStripMenuItem_Winver_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Name = "label1";
            // 
            // ScrollText_Timer1
            // 
            this.ScrollText_Timer1.Enabled = true;
            this.ScrollText_Timer1.Tick += new System.EventHandler(this.ScrollText_Timer1_Tick);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.GreenYellow;
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Name = "label4";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Adjustment.Properties.Resources.OleasterFruit;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Name = "label5";
            // 
            // AboutAurora
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutAurora";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TransparencyKey = System.Drawing.Color.Transparent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AboutAurora_FormClosing);
            this.Load += new System.EventHandler(this.AboutAurora_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.AboutAurora_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AboutAurora_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer waterTime;
        private System.Windows.Forms.Timer dropsTime;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Dynamic;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Weibo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Website;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_DxDiag;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Perfmon;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Resmon;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Msinfo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Exit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Winver;
        private System.Windows.Forms.Timer ScrollText_Timer1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadMind;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}