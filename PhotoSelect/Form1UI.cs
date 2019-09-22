using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoSelect {
    public partial class Form1: Form {

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

        private void ExpandTreeNode(TreeNode node) {
            if (node.ImageIndex == 0) {
                node.Nodes.Clear();
                node.ImageIndex = 1;
                CreateChildNodes(node);
            }
        }

        private void CreateChildNodes(TreeNode parentNode) {
            var parentInfo = new DirectoryInfo((string)parentNode.Tag);
            foreach (var directory in parentInfo
                .GetDirectories()
                .Where(d => d.Attributes.HasFlag(FileAttributes.Hidden) == false)
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

        private void DrawTreeNode(DrawTreeNodeEventArgs e) {
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
}
