using System;
using System.Data;
using System.Windows.Forms;

namespace QueryWorkbenchUI.UserControls.ContextMenus {
    public class ResultsGridColumnHeaderContextMenu : ContextMenu {
        private MenuItem _menuItemShowAllColumns;
        private MenuItem _separator = new MenuItem("-");

        public readonly DataGridView GridView;

        protected bool ShowAllMenuItemVisible {
            get {
                return _menuItemShowAllColumns.Visible;
            }
            set {
                _menuItemShowAllColumns.Visible = value;
                _separator.Visible = value;
            }
        }
        public ResultsGridColumnHeaderContextMenu(DataGridView gridView) : base() {
            var dataTable = gridView.DataSource as DataTable;
            if (dataTable == null) {
                throw new ArgumentNullException($"{nameof(gridView.DataSource)}");
            }

            GridView = gridView;

            _menuItemShowAllColumns = new MenuItem("Show all");
            _menuItemShowAllColumns.Click += showAllColumns_Click;
            _menuItemShowAllColumns.Visible = false;

            var dataColumns = dataTable.Columns;
            for (int j = 0; j < dataColumns.Count; j++) {
                var menuItem = new MenuItem(dataColumns[j].ColumnName);
                menuItem.Checked = GridView.Columns[dataColumns[j].ColumnName].Visible;
                menuItem.Click += toggleColumnVisibility;
                MenuItems.Add(menuItem);
            }
            
            ShowAllMenuItemVisible = !AllGridColumnsVisible();

            MenuItems.AddRange(new MenuItem[] { _separator, _menuItemShowAllColumns, });
        }

        private void toggleColumnVisibility(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            GridView.Columns[menuItem.Text].Visible = !GridView.Columns[menuItem.Text].Visible;
            menuItem.Checked = GridView.Columns[menuItem.Text].Visible;
            _menuItemShowAllColumns.Visible = !AllGridColumnsVisible();
            _separator.Visible = _menuItemShowAllColumns.Visible;
        }

        private bool AllGridColumnsVisible() {
            for (int j = 0; j < GridView.Columns.Count; j++) {
                if (!GridView.Columns[j].Visible) {
                    return false;
                }
            }
            return true;
        }

        private void showAllColumns_Click(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            for (int j = 0; j < GridView.Columns.Count; j++) {
                GridView.Columns[j].Visible = true;
            }

            foreach (MenuItem childMenuItem in MenuItems) {
                if (childMenuItem != _menuItemShowAllColumns) {
                    childMenuItem.Checked = true;
                }
            }
            ShowAllMenuItemVisible = false;
        }
    }
}
