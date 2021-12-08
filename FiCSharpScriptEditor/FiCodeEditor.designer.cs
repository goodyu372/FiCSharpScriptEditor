namespace FiCSharpScriptEditor
{
    partial class FiCodeEditor
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
            this.wpfEditorHost = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // wpfEditorHost
            // 
            this.wpfEditorHost.BackColor = System.Drawing.SystemColors.Control;
            this.wpfEditorHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpfEditorHost.Location = new System.Drawing.Point(0, 0);
            this.wpfEditorHost.Name = "wpfEditorHost";
            this.wpfEditorHost.Size = new System.Drawing.Size(647, 390);
            this.wpfEditorHost.TabIndex = 0;
            this.wpfEditorHost.Text = "elementHost1";
            this.wpfEditorHost.Visible = false;
            this.wpfEditorHost.Child = null;
            // 
            // FiCodeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.wpfEditorHost);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FiCodeEditor";
            this.Size = new System.Drawing.Size(647, 390);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost wpfEditorHost;
    }
}
