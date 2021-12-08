namespace FiCSharpScriptEditor
{
    partial class ReferenceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReferenceForm));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btSave = new System.Windows.Forms.Button();
            this.btReferenceGAC = new System.Windows.Forms.Button();
            this.btReferenceLocal = new System.Windows.Forms.Button();
            this.btClose = new System.Windows.Forms.Button();
            this.btDelete = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.c类型 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c名称 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c存在 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c路径 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.listView1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1154, 564);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Controls.Add(this.btSave, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btReferenceGAC, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btReferenceLocal, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btClose, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btDelete, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1146, 104);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // btSave
            // 
            this.btSave.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btSave.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btSave.Location = new System.Drawing.Point(463, 5);
            this.btSave.Margin = new System.Windows.Forms.Padding(5);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(219, 94);
            this.btSave.TabIndex = 26;
            this.btSave.Text = "保存";
            this.btSave.UseVisualStyleBackColor = false;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btReferenceGAC
            // 
            this.btReferenceGAC.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btReferenceGAC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btReferenceGAC.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btReferenceGAC.Location = new System.Drawing.Point(234, 5);
            this.btReferenceGAC.Margin = new System.Windows.Forms.Padding(5);
            this.btReferenceGAC.Name = "btReferenceGAC";
            this.btReferenceGAC.Size = new System.Drawing.Size(219, 94);
            this.btReferenceGAC.TabIndex = 23;
            this.btReferenceGAC.Text = "引用系统dll";
            this.btReferenceGAC.UseVisualStyleBackColor = false;
            this.btReferenceGAC.Click += new System.EventHandler(this.btReferenceGAC_Click);
            // 
            // btReferenceLocal
            // 
            this.btReferenceLocal.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btReferenceLocal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btReferenceLocal.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btReferenceLocal.Location = new System.Drawing.Point(5, 5);
            this.btReferenceLocal.Margin = new System.Windows.Forms.Padding(5);
            this.btReferenceLocal.Name = "btReferenceLocal";
            this.btReferenceLocal.Size = new System.Drawing.Size(219, 94);
            this.btReferenceLocal.TabIndex = 22;
            this.btReferenceLocal.Text = "引用本地dll";
            this.btReferenceLocal.UseVisualStyleBackColor = false;
            this.btReferenceLocal.Click += new System.EventHandler(this.btReferenceLocal_Click);
            // 
            // btClose
            // 
            this.btClose.BackColor = System.Drawing.Color.Tomato;
            this.btClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btClose.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btClose.Location = new System.Drawing.Point(921, 5);
            this.btClose.Margin = new System.Windows.Forms.Padding(5);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(220, 94);
            this.btClose.TabIndex = 25;
            this.btClose.Text = "关闭";
            this.btClose.UseVisualStyleBackColor = false;
            this.btClose.Click += new System.EventHandler(this.btClose_Click);
            // 
            // btDelete
            // 
            this.btDelete.BackColor = System.Drawing.Color.Tomato;
            this.btDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btDelete.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btDelete.Location = new System.Drawing.Point(692, 5);
            this.btDelete.Margin = new System.Windows.Forms.Padding(5);
            this.btDelete.Name = "btDelete";
            this.btDelete.Size = new System.Drawing.Size(219, 94);
            this.btDelete.TabIndex = 24;
            this.btDelete.Text = "删除";
            this.btDelete.UseVisualStyleBackColor = false;
            this.btDelete.Click += new System.EventHandler(this.btDelete_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.c类型,
            this.c名称,
            this.c存在,
            this.c路径});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(4, 116);
            this.listView1.Margin = new System.Windows.Forms.Padding(4);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1146, 444);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // c类型
            // 
            this.c类型.Text = "类型";
            this.c类型.Width = 100;
            // 
            // c名称
            // 
            this.c名称.Text = "名称";
            this.c名称.Width = 120;
            // 
            // c存在
            // 
            this.c存在.Text = "存在";
            this.c存在.Width = 100;
            // 
            // c路径
            // 
            this.c路径.Text = "路径";
            this.c路径.Width = 1500;
            // 
            // ReferenceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1154, 564);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ReferenceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ReferenceForm";
            this.Load += new System.EventHandler(this.ReferenceForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btDelete;
        private System.Windows.Forms.Button btReferenceGAC;
        private System.Windows.Forms.Button btReferenceLocal;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader c类型;
        private System.Windows.Forms.ColumnHeader c名称;
        private System.Windows.Forms.ColumnHeader c路径;
        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.ColumnHeader c存在;
    }
}