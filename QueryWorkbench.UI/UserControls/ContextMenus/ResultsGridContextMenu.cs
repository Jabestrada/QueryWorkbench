using QueryWorkbenchUI.Extensions;
using QueryWorkbenchUI.Orchestration;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace QueryWorkbenchUI.UserControls.ContextMenus {
    public class ResultsGridContextMenu : ContextMenu {
        private readonly IResultsPaneView Owner;

        private MenuItem _menuItemGroupColumns;
        private MenuItem _menuItemSaveToCsv;

        private MenuItem _separator = new MenuItem("-");
        private MenuItem _menuItemShowAllColumns;
        private MenuItem _menuItemToggleVisibilityAllColumns;

        public readonly DataGridView GridView;

        public ResultsGridContextMenu(IResultsPaneView owner, DataGridView gridView) : base() {
            var dataTable = gridView.DataSource as DataTable;
            if (dataTable == null) {
                throw new ArgumentNullException($"{nameof(gridView.DataSource)}");
            }

            Owner = owner;
            owner.OnResultsCountChanged += Owner_OnResultsCountChanged;

            GridView = gridView;

            _menuItemGroupColumns = createMenuItemGroupShowHideColumns(dataTable);

            MenuItems.Add(_menuItemGroupColumns);

            _menuItemSaveToCsv = new MenuItem("Save to CSV...");
            _menuItemSaveToCsv.Click += _menuItemSaveToCsv_Click;
            MenuItems.Add(_menuItemSaveToCsv);
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
        private void Owner_OnResultsCountChanged(object sender, ResultsCountChangedArgs e) {
            _menuItemSaveToCsv.Enabled = e.NewCount > 0;
        }

        private void _menuItemSaveToCsv_Click(object sender, EventArgs e) {
            var newFileDialog = new OpenFileDialog {
                CheckFileExists = false,
                Filter = "CSV| *.csv|All files (*.*)|*.*",
                FileName = $"{Owner.Text}.csv"
            };
            if (newFileDialog.ShowDialog() == DialogResult.OK) {
                using (StreamWriter sw = new StreamWriter(newFileDialog.FileName, false)) {
                    GridView.WriteDelimited(sw);
                    sw.Close();
                }
                MessageBox.Show($"Results saved to {newFileDialog.FileName}", "File saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private MenuItem createMenuItemGroupShowHideColumns(DataTable dataTable) {
            var menuItemGroupColumns = new MenuItem("Show/Hide Columns");
            var dataColumns = dataTable.Columns;
            for (int j = 0; j < dataColumns.Count; j++) {
                var menuItem = new MenuItem(dataColumns[j].ColumnName);
                menuItem.Checked = GridView.Columns[dataColumns[j].ColumnName].Visible;
                menuItem.Click += toggleColumnVisibility;
                menuItem.Tag = new MenuItemMetadata(MenuItemActionType.ColumnHeaderVisibility);
                menuItemGroupColumns.MenuItems.Add(menuItem);
            }

            _menuItemToggleVisibilityAllColumns = new MenuItem("Toggle column visibility");
            _menuItemToggleVisibilityAllColumns.Click += menuItemToggleVisibilityAllColumns_Click;

            _menuItemShowAllColumns = new MenuItem("Show all");
            _menuItemShowAllColumns.Click += showAllColumns_Click;

            _menuItemShowAllColumns.Enabled = !GridView.AllColumnsShown();

            menuItemGroupColumns.MenuItems.AddRange(new MenuItem[] { _separator, 
                                                                    _menuItemShowAllColumns, 
                                                                    _menuItemToggleVisibilityAllColumns });

            return menuItemGroupColumns;
        }

        private void menuItemToggleVisibilityAllColumns_Click(object sender, EventArgs e) {
            foreach (MenuItem menuItem in _menuItemGroupColumns.MenuItems) {
                MenuItemMetadata metaData = menuItem.Tag as MenuItemMetadata;
                if (metaData == null) continue;

                if (metaData.ActionType == MenuItemActionType.ColumnHeaderVisibility) {
                    menuItem.Checked = GridView.ToggleColumnVisibility(menuItem.Text);
                }
            }

            _menuItemShowAllColumns.Enabled = !GridView.AllColumnsShown();
        }

        private void toggleColumnVisibility(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            menuItem.Checked = GridView.ToggleColumnVisibility(menuItem.Text);
            _menuItemShowAllColumns.Enabled = !GridView.AllColumnsShown();
        }

        private void showAllColumns_Click(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            for (int j = 0; j < GridView.Columns.Count; j++) {
                GridView.Columns[j].Visible = true;
            }

            foreach (MenuItem childMenuItem in _menuItemGroupColumns.MenuItems) {
                MenuItemMetadata menuItemMetadata = childMenuItem.Tag as MenuItemMetadata;
                if (menuItemMetadata == null) continue;

                if (menuItemMetadata.ActionType == MenuItemActionType.ColumnHeaderVisibility) {
                    childMenuItem.Checked = true;
                }
            }
            _menuItemShowAllColumns.Enabled = false;
        }
        #endregion non-public
    }
}
