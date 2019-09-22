using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoSelect {
    public partial class Form1 : Form {
        List<RecentFolder> recentFolders = new List<RecentFolder>();
        RecentFolder currentFolder = null;

        public Form1() {
            InitializeComponent();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e) {
            string original, selected;

            folderBrowserDialog1.Description = "First select the folder where your original photos are located";
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK) {
                original = folderBrowserDialog1.SelectedPath;
                folderBrowserDialog1.Description = "Now select the folder where your selected photos are located";
                if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK) {
                    selected = folderBrowserDialog1.SelectedPath;
                    if (original == selected) {
                        MessageBox.Show("The original and selected folders cannot be the same", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    } else {
                        RecentFolder r = new RecentFolder(original, selected);
                        recentFolders.Add(r);
                        OpenFolder(r);
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            FormDataSave();
        }

        private void Form1_Load(object sender, EventArgs e) {
            FormDataRestore();
        }

        private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
            ExpandTreeNode(e.Node);
        }

        private void RecentFolder_Click(object sender, EventArgs e) {
            var item = sender as ToolStripMenuItem;
            foreach (RecentFolder r in this.recentFolders) {
                if (r.original == item.Text) {
                    OpenFolder(r);
                    return;
                }
            }
        }

        private void ListView1_DrawItem(object sender, DrawListViewItemEventArgs e) {

        }

        private void TreeView1_DrawNode(object sender, DrawTreeNodeEventArgs e) {
            DrawTreeNode(e);
        }

    }
}
