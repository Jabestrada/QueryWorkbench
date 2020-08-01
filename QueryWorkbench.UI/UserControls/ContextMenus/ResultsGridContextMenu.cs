using System;
using System.Data;
using System.Windows.Forms;

namespace QueryWorkbenchUI.UserControls.ContextMenus {
    public class ResultsGridContextMenu : ContextMenu {
        private MenuItem _menuItemShowAllColumns;
        private MenuItem _menuItemToggleVisibilityAllColumns;

        private MenuItem _separator = new MenuItem("-");

        public readonly DataGridView GridView;

        public ResultsGridContextMenu(DataGridView gridView) : base() {
            var dataTable = gridView.DataSource as DataTable;
            if (dataTable == null) {
                throw new ArgumentNullException($"{nameof(gridView.DataSource)}");
            }

            GridView = gridView;

            _menuItemToggleVisibilityAllColumns = new MenuItem("Toggle column visibility");
            _menuItemToggleVisibilityAllColumns.Click += _menuItemToggleVisibilityAllColumns_Click;

            _menuItemShowAllColumns = new MenuItem("Show all");
            _menuItemShowAllColumns.Click += showAllColumns_Click;

            var dataColumns = dataTable.Columns;
            for (int j = 0; j < dataColumns.Count; j++) {
                var menuItem = new MenuItem(dataColumns[j].ColumnName);
                menuItem.Checked = GridView.Columns[dataColumns[j].ColumnName].Visible;
                menuItem.Click += toggleColumnVisibility;
                menuItem.Tag = new MenuItemMetaData { ActionType = MenuItemActionType.ColumnHeaderVisibility };
                MenuItems.Add(menuItem);
            }

            _menuItemShowAllColumns.Visible = !AllGridColumnsShown;

            MenuItems.AddRange(new MenuItem[] { _separator, _menuItemShowAllColumns, _menuItemToggleVisibilityAllColumns });
        }

        private void _menuItemToggleVisibilityAllColumns_Click(object sender, EventArgs e) {
            foreach (MenuItem menuItem in MenuItems) {
                MenuItemMetaData metaData = menuItem.Tag as MenuItemMetaData;
                if (metaData == null) continue;

                if (metaData.ActionType == MenuItemActionType.ColumnHeaderVisibility) {
                    GridView.Columns[menuItem.Text].Visible = !GridView.Columns[menuItem.Text].Visible;
                    menuItem.Checked = GridView.Columns[menuItem.Text].Visible;
                }
            }

            _menuItemShowAllColumns.Visible = !AllGridColumnsShown;
        }

        public void HandleRightMouseClick(object sender, MouseEventArgs e) {
            Show(GridView, GridView.PointToClient(Cursor.Position));

            //var rightClickedWhich = GridView.HitTest(e.X, e.Y).Type;
            //if (rightClickedWhich == DataGridViewHitTestType.ColumnHeader) {
            //    Show(GridView, GridView.PointToClient(Cursor.Position));
            //}
            //else {
            //    if (AllGridColumnsHidden) {
            //        ShowAllMenuItemVisible = true;
            //        Show(GridView, GridView.PointToClient(Cursor.Position));
            //    }
            //}
        }

        #region non-public
        private void toggleColumnVisibility(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            GridView.Columns[menuItem.Text].Visible = !GridView.Columns[menuItem.Text].Visible;
            menuItem.Checked = GridView.Columns[menuItem.Text].Visible;
            _menuItemShowAllColumns.Visible = !AllGridColumnsShown;
            _separator.Visible = _menuItemShowAllColumns.Visible;
        }

        private bool AllGridColumnsShown {
            get {
                for (int j = 0; j < GridView.Columns.Count; j++) {
                    if (!GridView.Columns[j].Visible) {
                        return false;
                    }
                }
                return true;
            }
        }

        private void showAllColumns_Click(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            for (int j = 0; j < GridView.Columns.Count; j++) {
                GridView.Columns[j].Visible = true;
            }

            foreach (MenuItem childMenuItem in MenuItems) {
                MenuItemMetaData menuItemMetadata = childMenuItem.Tag as MenuItemMetaData;
                if (menuItemMetadata == null) continue;

                if (menuItemMetadata.ActionType == MenuItemActionType.ColumnHeaderVisibility) {
                    childMenuItem.Checked = true;
                }
            }
            _menuItemShowAllColumns.Visible = false;
        }

        #endregion non-public
    }
}
