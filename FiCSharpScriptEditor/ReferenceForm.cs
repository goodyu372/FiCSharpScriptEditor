using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FiCSharpScriptEditor
{
    public partial class ReferenceForm : Form
    {
        public ReferenceForm()
        {
            InitializeComponent();
        }

        private void ReferenceForm_Load(object sender, EventArgs e)
        {
            ReferenceManager.LoadData();
            UpdateReferencesView();
        }

        private void UpdateReferencesView() 
        {
            this.listView1.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度  
            this.listView1.Items.Clear();

            foreach (var item in ReferenceManager.ListReferences)
            {
                ListViewItem listViewItem = this.listView1.Items.Add(item.Type.ToString());
                listViewItem.SubItems.Add(item.Name);
                listViewItem.SubItems.Add(item.Exist.ToString());
                listViewItem.SubItems.Add(item.Location);

                if (!item.Exist)
                {
                    listViewItem.BackColor = Color.Tomato;
                }
            }

            foreach (ColumnHeader column in this.listView1.Columns)
            {
                column.Width = -2;
            }
            this.listView1.EndUpdate();  //结束数据处理，UI界面一次性绘制。
        }

        private void btReferenceLocal_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择dll";
            dialog.Filter = "dll(*.dll)|*.dll";
            dialog.InitialDirectory = @"D:\Fi";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string[] files = dialog.FileNames;
                foreach (string file in files)
                {
                    ReferenceManager.AddReference(file, ReferenceType.Local);
                }
                UpdateReferencesView();
            }
        }

        private void btReferenceGAC_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择dll";
            dialog.Filter = "dll(*.dll)|*.dll";
            dialog.InitialDirectory = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string[] files = dialog.FileNames;
                foreach (string file in files)
                {
                    ReferenceManager.AddReference(file, ReferenceType.System_GAC);
                }
                UpdateReferencesView();
            }
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选中一行再操作！");
                return;
            }
            if (this.listView1.SelectedItems.Count > 1)
            {
                MessageBox.Show("请选中单行再操作！");
                return;
            }
            ListViewItem listViewItem = this.listView1.SelectedItems[0];
            int index = listViewItem.Index;
            string name = listViewItem.SubItems[1].Text;
            if (MessageBox.Show($"确定要删除{name}的引用吗？","删除确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ReferenceManager.RemoveReference(index);
                UpdateReferencesView();
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            ReferenceManager.SaveData();
            MessageBox.Show("保存成功");
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
