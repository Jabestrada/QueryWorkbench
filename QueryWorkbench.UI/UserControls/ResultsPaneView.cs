using QueryWorkbenchUI.Orchestration;
using QueryWorkbenchUI.UserControls.ContextMenus;
using System;
using System.Data;
using System.Windows.Forms;

namespace QueryWorkbenchUI.UserControls {
    public partial class ResultsPaneView : UserControl, IResultsPaneView  {

        private DataTable _sourceDataTable;
        private int _containerIndex, _oldCount, _newCount;

        public event EventHandler<ResultsCountChangedArgs> OnResultsCountChanged;

        private ResultsGridContextMenu _resultsGridContextMenu;

        #region ctors
        public ResultsPaneView() {
            InitializeComponent();
        }
        public ResultsPaneView(string tabText, DataTable sourceDataTable) : this() {
            if (sourceDataTable == null) {
                return;
            }

            Text = tabText;
            _oldCount = _sourceDataTable == null ? 0 : _sourceDataTable.DefaultView.Count;
            _newCount = sourceDataTable.DefaultView.Count;

            SetDataSource(sourceDataTable);
        }
        #endregion

        #region IResultsView
        public override string Text { get; set; }
        
        public bool IsOutputPaneVisible {
            get {
                return !splitContainer1.Panel2Collapsed;
            }
            set {
                splitContainer1.Panel2Collapsed = !value;
            }
        }

        public void ApplyFilter() {
            applyFilterInternal();
            OnResultsCountChanged?.Invoke(this, new ResultsCountChangedArgs(_oldCount, _newCount, _containerIndex));
        }
        #endregion IResultsView

        public void SetDataSource(DataTable sourceDataTable) {
            _oldCount = _sourceDataTable == null ? 0 : _sourceDataTable.DefaultView.Count;

            _sourceDataTable = sourceDataTable;

            gridResults.Columns.Clear();
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

            _resultsGridContextMenu = new ResultsGridContextMenu(this, gridResults);
            
            _newCount = _sourceDataTable.DefaultView.Count;
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

        private void gridResults_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                _resultsGridContextMenu.HandleRightMouseClick(sender, e);
            }
        }

        private void gridResults_CellEnter(object sender, DataGridViewCellEventArgs e) {
            writeCurrentCellValue(e);
        }
        #endregion non-public

    }
}
