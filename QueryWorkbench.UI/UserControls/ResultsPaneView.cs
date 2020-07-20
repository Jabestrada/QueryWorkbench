using QueryWorkbenchUI.Orchestration;
using System;
using System.Data;
using System.Windows.Forms;

namespace QueryWorkbenchUI.UserControls {
    public partial class ResultsPaneView : UserControl, IResultsView {
        private DataTable _sourceDataTable;
        private int _containerIndex, _oldCount, _newCount;

        public event EventHandler<ResultsCountChangedArgs> OnResultsCountChanged;

        public ResultsPaneView() {
            InitializeComponent();
        }
        public ResultsPaneView(DataTable sourceDataTable) : this() {
            if (sourceDataTable == null) {
                return;
            }
            _oldCount = _sourceDataTable == null ? 0 : _sourceDataTable.DefaultView.Count;
            _newCount = sourceDataTable.DefaultView.Count;

            _sourceDataTable = sourceDataTable;

            gridResults.DataSource = _sourceDataTable;
            splitContainer1.Panel1Collapsed = _sourceDataTable.DefaultView.Count == 0;
            if (splitContainer1.Panel1Collapsed) {
                splitContainer1.Panel1.Hide();
            }
            else {
                splitContainer1.Panel1.Show();
            }
        }

        public void ApplyFilter() {
            applyFilter();
            OnResultsCountChanged?.Invoke(this, new ResultsCountChangedArgs(_oldCount, _newCount, _containerIndex));
        }

        public ResultsPaneView WithDockStyle(DockStyle dockStyle) {
            Dock = dockStyle;
            return this;
        }
        public ResultsPaneView WithContainerIndex(int containerIndex) {
            _containerIndex = containerIndex;
            return this;
        }

        private void applyFilter() {
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
            var dt = gridResults.DataSource as DataTable;
            if (dt == null) {
                return;
            }
            if (e.RowIndex > dt.Rows.Count - 1) {
                return;
            }
            txtOutput.Text = dt.Rows[e.RowIndex][e.ColumnIndex].ToString();
        }

        private void gridResults_CellEnter(object sender, DataGridViewCellEventArgs e) {
            writeCurrentCellValue(e);
        }

        public ResultsPaneView WithResultsCountChangedHandler(EventHandler<ResultsCountChangedArgs> resultsCountChangedHandler) {
            OnResultsCountChanged = resultsCountChangedHandler;
            OnResultsCountChanged?.Invoke(this, new ResultsCountChangedArgs(_oldCount, _newCount, _containerIndex));
            return this;
        }
    }
}
