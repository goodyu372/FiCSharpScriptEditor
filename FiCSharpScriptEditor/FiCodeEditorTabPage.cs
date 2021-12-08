using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FiCSharpScriptEditor
{
	/// <summary>
	/// FiEditorTabPage
	/// </summary>
	public class FiCodeEditorTabPage : TabPage
    {
        private FiCodeEditor fiCodeEditor;

		/// <summary>
		/// 构造
		/// </summary>
		/// <param name="fiCodeEditor"></param>
		public FiCodeEditorTabPage(FiCodeEditor fiCodeEditor)
		{
			fiCodeEditor.Dock = DockStyle.Fill;
			this.fiCodeEditor = fiCodeEditor;
			this.Controls.Add(fiCodeEditor);
		}

		/// <summary>
		/// 代码编辑器
		/// </summary>
		public FiCodeEditor FiCodeEditor
		{
			get { return fiCodeEditor; }
		}
	}
}
