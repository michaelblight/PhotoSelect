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

namespace PhotoSelect
{
    public partial class Form1 : Form
    {
        List<RecentFolder> recentFolders = new List<RecentFolder>();
        RecentFolder currentFolder = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void FormDataSave()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Settings.Default.WindowState = FormWindowState.Normal; // Don't save as minimized
            } else {
                Settings.Default.WindowState = this.WindowState;
            }

            if (this.WindowState == FormWindowState.Normal)
                Settings.Default.WindowPosition = this.DesktopBounds; // Don't save if maximized/minimized

            Settings.Default.SplitterPosition = splitContainer1.SplitterDistance;

            Settings.Default.RecentFolders = ConvertToList(this.recentFolders);
            Console.WriteLine("Saved " + Settings.Default.RecentFolders.Count.ToString() + " recent folders");

            Settings.Default.Save();
        }

        private void FormDataRestore()
        {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.WindowsDefaultBounds;

            if (Settings.Default.WindowPosition != Rectangle.Empty &&
                IsVisibleOnAnyScreen(Settings.Default.WindowPosition))
            {
                this.StartPosition = FormStartPosition.Manual;
                this.DesktopBounds = Settings.Default.WindowPosition;
                this.WindowState = Settings.Default.WindowState;
            }
            else
            {
                this.StartPosition = FormStartPosition.WindowsDefaultLocation;
            }

            int distance = Settings.Default.SplitterPosition;
            if (distance > 0)
            {
                splitContainer1.SplitterDistance = distance;
            }

            this.recentFolders = ConvertFromList(Settings.Default.RecentFolders as List<string>);
            UpdateRecentFolders();
            Console.WriteLine("Loaded " + recentFolders.Count.ToString() + " recent folders");
        }

        private bool IsVisibleOnAnyScreen(Rectangle rect)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect))
                {
                    return true;
                }
            }

            return false;
        }


        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string original, selected;

            folderBrowserDialog1.Description = "First select the folder where your original photos are located";
            if (this.folderBrowserDialog1.ShowDialog()  == DialogResult.OK)
            {
                original = folderBrowserDialog1.SelectedPath;
                folderBrowserDialog1.Description = "Now select the folder where your selected photos are located";
                if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormDataSave();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FormDataRestore();
        }

        private void OpenFolder(RecentFolder folder) {
            this.currentFolder = folder;
            treeView1.Nodes.Clear();
            var rootInfo = new DirectoryInfo(this.currentFolder.original);
            var rootNode = treeView1.Nodes.Add(rootInfo.Name);
            rootNode.Tag = rootInfo.FullName;
            rootNode.ImageIndex = 0;
            if (rootInfo.GetDirectories().First() != null) {
                rootNode.Nodes.Add("Please wait...");
            }
        }

        private void CreateChildNodes(TreeNode parentNode) {
            var parentInfo = new DirectoryInfo((string)parentNode.Tag);
            foreach (var directory in parentInfo
                .GetDirectories()
                .Where(d => d.Attributes.HasFlag(FileAttributes.Hidden) == false )
                .OrderBy(d => d.Name)) {
                var childPath = directory.FullName;
                TreeNode childNode = parentNode.Nodes.Add(directory.Name);
                childNode.Tag = childPath;
                childNode.ImageIndex = 0;
                var childInfo = new DirectoryInfo(childPath);
                if (childInfo.GetDirectories().Length > 0) {
                    childNode.Nodes.Add("Please wait...");
                }
            }
        }

        private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
            var expand = e.Node;
            if (expand.ImageIndex == 0) {
                expand.Nodes.Clear();
                expand.ImageIndex = 1;
                CreateChildNodes(expand);
            }
        }

        private List<string> ConvertToList(List<RecentFolder> recentFolders) {
            List<string> result = new List<string>();
            recentFolders.ForEach(r => result.Add(r.original + "=" + r.selected));
            return result;
        }

        private List<RecentFolder> ConvertFromList(List<string> recentFolders) {
            List<RecentFolder> result = new List<RecentFolder>();
            if (recentFolders == null) return result;

            recentFolders.ForEach(r => {
               string[] pair = r.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length >= 2) result.Add(new RecentFolder(pair[0], pair[1]));
            });
            return result;
        }

        private void UpdateRecentFolders() {
            recentToolStripMenuItem.DropDownItems.Clear();
            this.recentFolders.ForEach(r => {
                var item = recentToolStripMenuItem.DropDownItems.Add(r.original);
                item.Click += RecentFolder_Click;
            });
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
            TreeNodeStates state = e.State;
            Font font = e.Node.NodeFont ?? e.Node.TreeView.Font;
            Color fore = e.Node.ForeColor;
            Color back = SystemColors.Highlight;
            if (fore == Color.Empty) fore = e.Node.TreeView.ForeColor;
            if (e.Node == e.Node.TreeView.SelectedNode) {
                fore = SystemColors.HighlightText;
                e.Graphics.FillRectangle(new SolidBrush(back), e.Bounds);
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, fore, back);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, fore, back, TextFormatFlags.GlyphOverhangPadding);
            } else {
                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, fore, TextFormatFlags.GlyphOverhangPadding);
            }
        }
    }
    public class RecentFolder {
        public string original, selected;
        public RecentFolder(string original, string selected) {
            this.original = original;
            this.selected = selected;
        }
    }

}
