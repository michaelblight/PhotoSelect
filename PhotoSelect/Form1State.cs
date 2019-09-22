using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoSelect {
    public partial class Form1 : Form {
        private void FormDataSave() {
            if (this.WindowState == FormWindowState.Minimized) {
                Settings.Default.WindowState = FormWindowState.Normal; // Don't save as minimized
            } else {
                Settings.Default.WindowState = this.WindowState;
            }

            if (this.WindowState == FormWindowState.Normal)
                Settings.Default.WindowPosition = this.DesktopBounds; // Don't save if maximized/minimized

            Settings.Default.SplitterPosition = splitContainer1.SplitterDistance;

            Settings.Default.RecentFolders = ConvertToList(this.recentFolders);

            Settings.Default.Save();
        }

        private void FormDataRestore() {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.WindowsDefaultBounds;

            if (Settings.Default.WindowPosition != Rectangle.Empty &&
                IsVisibleOnAnyScreen(Settings.Default.WindowPosition)) {
                this.StartPosition = FormStartPosition.Manual;
                this.DesktopBounds = Settings.Default.WindowPosition;
                this.WindowState = Settings.Default.WindowState;
            } else {
                this.StartPosition = FormStartPosition.WindowsDefaultLocation;
            }

            int distance = Settings.Default.SplitterPosition;
            if (distance > 0) splitContainer1.SplitterDistance = distance;

            this.recentFolders = ConvertFromList(Settings.Default.RecentFolders as List<string>);
            UpdateRecentFolders();
            Console.WriteLine("Loaded " + recentFolders.Count.ToString() + " recent folders");
        }

        private bool IsVisibleOnAnyScreen(Rectangle rect) {
            foreach (Screen screen in Screen.AllScreens) {
                if (screen.WorkingArea.IntersectsWith(rect)) {
                    return true;
                }
            }

            return false;
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

    }

    public class RecentFolder {
        public string original, selected;
        public RecentFolder(string original, string selected) {
            this.original = original;
            this.selected = selected;
        }
    }
}
