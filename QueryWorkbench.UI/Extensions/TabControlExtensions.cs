using System.Windows.Forms;

namespace QueryWorkbenchUI.Extensions {
    public static class TabControlExtensions {
        public static void SelectNextTab(this TabControl tabControl) {
            if (tabControl.TabPages == null || tabControl.TabPages.Count <= 1) return;

            tabControl.SelectedTab = tabControl.TabPages[(tabControl.SelectedIndex + 1) % tabControl.TabCount];
        }
        public static void SelectPreviousTab(this TabControl tabControl) {
            if (tabControl.TabPages == null || tabControl.TabPages.Count <= 1) {
                return;
            }

            tabControl.SelectedTab = tabControl.TabPages[tabControl.SelectedIndex == 0 ? tabControl.TabCount - 1 : tabControl.SelectedIndex - 1];
        }
    }
}
