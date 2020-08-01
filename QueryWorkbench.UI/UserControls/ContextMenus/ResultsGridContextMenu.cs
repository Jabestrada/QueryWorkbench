using QueryWorkbenchUI.Extensions;
using QueryWorkbenchUI.Orchestration;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QueryWorkbenchUI.UserControls.ContextMenus {
    public class ResultsGridContextMenu : ContextMenu {
        private readonly IResultsPaneView Owner;
        public readonly DataGridView GridView;

        private bool _isRowDiffApplied;
        protected bool IsRowDiffApplied {
            get {
                return _isRowDiffApplied;
            }
            set {
                _isRowDiffApplied = value;
                if (_menuItemRowDiffClear != null) {
                    _menuItemRowDiffClear.Enabled = value;
                }
            }
        }

        #region Module-level menu items
        private MenuItem _menuItemGroupColumns;

        private MenuItem _separator = new MenuItem("-");
        private MenuItem _menuItemShowAllColumns;
        private MenuItem _menuItemToggleVisibilityAllColumns;

        private MenuItem _menuItemGroupRowDiff;
        private MenuItem _menuItemRowDiffClear;



        private MenuItem _menuItemSaveToCsv;
        #endregion Module-level menu items

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


            _menuItemGroupRowDiff = new MenuItem("Row Diff");

            var menuItemRowDiffApply = new MenuItem("Apply");
            menuItemRowDiffApply.Click += menuItemRowDiffApply_Click;

            _menuItemRowDiffClear = new MenuItem("Clear");
            _menuItemRowDiffClear.Click += menuItemRowDiffClear_Click;
            _menuItemGroupRowDiff.MenuItems.AddRange(new MenuItem[] { menuItemRowDiffApply, _menuItemRowDiffClear });

            _menuItemSaveToCsv = new MenuItem("Save to CSV...");
            _menuItemSaveToCsv.Click += _menuItemSaveToCsv_Click;

            MenuItems.AddRange(new MenuItem[] { _menuItemGroupRowDiff, _menuItemSaveToCsv });
            
            IsRowDiffApplied = false;
        }


        private void menuItemRowDiffClear_Click(object sender, EventArgs e) {
            if (IsRowDiffApplied) {
                revertCellStyles();
            }
            IsRowDiffApplied = false;
        }

        private void menuItemRowDiffApply_Click(object sender, EventArgs e) {
            if (GridView.SelectedRows.Count != 2) {
                MessageBox.Show("Row Diff requires exactly 2 results rows to be selected.", "Row Diff Requirement", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (IsRowDiffApplied) {
                revertCellStyles();
            }

            var noDiffCellStyle = new DataGridViewCellStyle {
                ForeColor = Color.Black,
                BackColor = Color.LightYellow,
            };

            var withDiffCellStyle = new DataGridViewCellStyle {
                ForeColor = Color.Red,
                BackColor = Color.LightYellow
            };

            for (int rowIndex = 0; rowIndex < GridView.Rows.Count; rowIndex++) {
                for (int colIndex = 0; colIndex < GridView.Columns.Count; colIndex++) {
                    if (!GridView.Columns[colIndex].Visible) continue;

                    var sameValues = GridView.SelectedRows[0].Cells[colIndex].Value.ToString() ==
                                     GridView.SelectedRows[1].Cells[colIndex].Value.ToString();
                    GridView.SelectedRows[0].Cells[colIndex].Style = sameValues ? noDiffCellStyle : withDiffCellStyle;
                    GridView.SelectedRows[1].Cells[colIndex].Style = sameValues ? noDiffCellStyle : withDiffCellStyle;
                }
            }

            GridView.ClearSelection();
            IsRowDiffApplied = true;
        }

        private void revertCellStyles() {
            for (int rowIndex = 0; rowIndex < GridView.Rows.Count; rowIndex++) {
                for (int colIndex = 0; colIndex < GridView.Columns.Count; colIndex++) {
                    if (GridView.Rows[rowIndex].Cells[colIndex].HasStyle) {
                        GridView.Rows[rowIndex].Cells[colIndex].Style = null;
                    }
                }
            }
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
