using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FiCSharpScriptEditor
{
    public partial class MainForm : Form
	{
        public delegate void TabSelectChangeDelegate(string strTabName);
        public static event TabSelectChangeDelegate TabSelectChangeEvent;

        public MainForm()
		{
			InitializeComponent();
			this.fileTabControl.SelectedIndexChanged += this.FileTabControlSelectedIndexChanged;
		}

		private void FileTabControlSelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.fileTabControl.SelectedTab != null)
			{
				TabSelectChangeEvent?.Invoke(this.fileTabControl.SelectedTab.Text);
			}
		}

		public FiCodeEditor ActiveFiCodeEditor => this.ActiveFiEditorTabPage?.FiCodeEditor;

		public FiCodeEditorTabPage ActiveFiEditorTabPage => this.fileTabControl.SelectedTab as FiCodeEditorTabPage;

		public FiCodeEditor GetFiCodeEditor(string name)
		{
			foreach (TabPage tabPage in this.fileTabControl.TabPages)
			{
				FiCodeEditorTabPage fiEditorTabPage = tabPage as FiCodeEditorTabPage;
				if (fiEditorTabPage.Text == name)
				{
					return fiEditorTabPage.FiCodeEditor;
				}
			}

			return null;
		}

		public async Task OpenOrLoadFile(string csFileFullName)
		{
            if (!File.Exists(csFileFullName))
            {
				return;
            }

			string name = Path.GetFileName(csFileFullName);
            foreach (FiCodeEditorTabPage page in this.fileTabControl.TabPages)
            {
				if (page.Text == name)
				{
					this.fileTabControl.SelectedTab = page;
					return;
				}
			}

			await LoadFile(csFileFullName);
		}

		private async Task LoadFile(string csFileFullName)
		{
			// Create a new tab page.
			FiCodeEditor fiCodeEditor = new FiCodeEditor();

			FiCodeEditorTabPage fiCodeEditorTabPage = new FiCodeEditorTabPage(fiCodeEditor);
			string name = Path.GetFileName(csFileFullName);
			fiCodeEditorTabPage.Text = name;

			this.fileTabControl.TabPages.Add(fiCodeEditorTabPage);
			this.fileTabControl.SelectedTab = fiCodeEditorTabPage;

			// Load file
			await fiCodeEditor.Initialize().ConfigureAwait(true);
			fiCodeEditor.LoadFile(csFileFullName);
		}

		public void SaveAll() 
		{
			foreach (FiCodeEditorTabPage fiCodeEditorTabPage in this.fileTabControl.TabPages)
			{
				 fiCodeEditorTabPage.FiCodeEditor.SaveFile();
			}
		}

        private void referencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReferenceForm referenceForm = new ReferenceForm();
            referenceForm.ShowDialog();
        }

        private async void fileOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.CheckFileExists = true;
				dialog.Filter = "cs(*.cs)|*.cs";
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					foreach (string fileName in dialog.FileNames)
					{
						await OpenOrLoadFile(fileName);
					}
				}
			}
		}

		private void fileCloseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.ActiveFiEditorTabPage != null)
			{
				this.fileTabControl.TabPages.Remove(this.ActiveFiEditorTabPage);
			}
		}
		private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.ActiveFiCodeEditor?.SaveFile();
		}

		private async void 格式化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (this.ActiveFiCodeEditor == null)
				return;

			await this.ActiveFiCodeEditor.FormatDocumentAsync();
		}

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
			this.ActiveFiCodeEditor?.Undo();
		}

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
			this.ActiveFiCodeEditor?.Redo();
		}

        private async void 注释ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.ActiveFiCodeEditor == null)
				return;

			await this.ActiveFiCodeEditor.CommentUncommentSelectionAsync(CommentAction.Comment);
		}

        private async void 取消注释ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.ActiveFiCodeEditor == null)
				return;

			await this.ActiveFiCodeEditor.CommentUncommentSelectionAsync(CommentAction.Uncomment);
		}
    }
}