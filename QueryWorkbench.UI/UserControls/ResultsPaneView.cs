using QueryWorkbenchUI.Orchestration;
using System;
using System.Data;
using System.Windows.Forms;

namespace QueryWorkbenchUI.UserControls {
    public partial class ResultsPaneView : UserControl, IResultsView {
        private DataTable _sourceDataTable;
        private int _containerIndex, _oldCount, _newCount;

        public event EventHandler<ResultsCountChangedArgs> OnResultsCountChanged;
        private ContextMenu _resulsGridViewContextMenu;
        private MenuItem _menuItemShowAll;

        #region ctors
        public ResultsPaneView() {
            InitializeComponent();
            _menuItemShowAll = new MenuItem("Show all");
            _menuItemShowAll.Click += showAllColumns_Click;
            _menuItemShowAll.Visible = false;
        }
        public ResultsPaneView(DataTable sourceDataTable) : this() {
            if (sourceDataTable == null) {
                return;
            }
            _oldCount = _sourceDataTable == null ? 0 : _sourceDataTable.DefaultView.Count;
            _newCount = sourceDataTable.DefaultView.Count;

            SetDataSource(sourceDataTable);
        }
        #endregion

        public void SetDataSource(DataTable sourceDataTable) {
            _sourceDataTable = sourceDataTable;

            gridResults.DataSource = _sourceDataTable;
            splitContainer1.Panel1Collapsed = _sourceDataTable.DefaultView.Count == 0;
            if (splitContainer1.Panel1Collapsed) {
                splitContainer1.Panel1.Hide();
                txtOutput.Text = "No records found";
                txtOutput.ReadOnly = true;
            }
            else {
                splitContainer1.Panel1.Show();
                txtOutput.Clear();
                txtOutput.ReadOnly = false;
            }
            setResultsGridViewContextMenu();
        }

        public void ApplyFilter() {
            applyFilterInternal();
            OnResultsCountChanged?.Invoke(this, new ResultsCountChangedArgs(_oldCount, _newCount, _containerIndex));
        }

        #region Fluent-style setters
        public ResultsPaneView WithDockStyle(DockStyle dockStyle) {
            Dock = dockStyle;
            return this;
        }
        public ResultsPaneView WithContainerIndex(int containerIndex) {
            _containerIndex = containerIndex;
            return this;
        }
        public ResultsPaneView WithResultsCountChangedHandler(EventHandler<ResultsCountChangedArgs> resultsCountChangedHandler) {
            OnResultsCountChanged = resultsCountChangedHandler;
            OnResultsCountChanged?.Invoke(this, new ResultsCountChangedArgs(_oldCount, _newCount, _containerIndex));
            return this;
        }
        #endregion Fluent-style setters

        #region non-public

        private void setResultsGridViewContextMenu() {
            _resulsGridViewContextMenu = new ContextMenu();

            for (int j = 0; j < _sourceDataTable.Columns.Count; j++) {
                var menuItem = new MenuItem(_sourceDataTable.Columns[j].ColumnName);
                menuItem.Checked = true;
                menuItem.Click += toggleColumnVisibility;
                _resulsGridViewContextMenu.MenuItems.Add(menuItem);
            }

            MenuItem separator = new MenuItem("-");
            separator.Tag = false;

            _resulsGridViewContextMenu.MenuItems.AddRange(new MenuItem[] { separator, _menuItemShowAll, });
        }

        private void showAllColumns_Click(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            for (int j = 0; j < gridResults.Columns.Count; j++) {
                gridResults.Columns[j].Visible = true;
            }

            foreach (MenuItem childMenuItem in _resulsGridViewContextMenu.MenuItems) {
                if (childMenuItem != _menuItemShowAll) {
                    childMenuItem.Checked = true;
                }
            }
            _menuItemShowAll.Visible = false;
        }

        private void toggleColumnVisibility(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            gridResults.Columns[menuItem.Text].Visible = !gridResults.Columns[menuItem.Text].Visible;
            menuItem.Checked = gridResults.Columns[menuItem.Text].Visible;
            if (!menuItem.Checked) {
                _menuItemShowAll.Visible = true;
            }
        }

        private void applyFilterInternal() {
            var dt = gridResults.DataSource as DataTable;
            _oldCount = dt.DefaultView.Count;
            try {
                dt.DefaultView.RowFilter = txtFilter.Text;
            }
            catch (Exception exc) {
                MessageBox.Show($"Error applying filter: {exc.Message}");
                return;
            }
            _newCount = dt.DefaultView.Count;
        }

        private void writeCurrentCellValue(DataGridViewCellEventArgs e) {
            var dataRowView = ((DataRowView)BindingContext[gridResults.DataSource].Current).Row;
            var index = dataRowView.Table.Rows.IndexOf(dataRowView);
            txtOutput.Text = dataRowView.Table.Rows[index][e.ColumnIndex].ToString();
        }

        private void gridResults_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {

                _resulsGridViewContextMenu.Show(gridResults, gridResults.PointToClient(Cursor.Position));
            }
        }

        private void gridResults_CellEnter(object sender, DataGridViewCellEventArgs e) {
            writeCurrentCellValue(e);
        }
        #endregion non-public

    }
}
