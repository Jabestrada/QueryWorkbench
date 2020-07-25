using QueryWorkbenchUI.UserControls;
using System.Data;
using System.Windows.Forms;

namespace QueryWorkbenchUI.Orchestration {
    public class TabbedResultsViewController {
        private readonly TabControl _tabContainer;
        public TabbedResultsViewController(TabControl tabContainer) {
            _tabContainer = tabContainer;
        }

        public void ApplyFilter() {
            if (_tabContainer.SelectedTab == null) {
                return;
            }

            IResultsView selectedResultsView = getChildControl<IResultsView>(_tabContainer.SelectedTab);

            if (selectedResultsView != null) {
                selectedResultsView.ApplyFilter();
            }
        }

        public void BindResults(DataSet ds) {
            var tabPageIndex = 0;
            foreach (DataTable dt in ds.Tables) {
                if (_tabContainer.TabCount > tabPageIndex) {
                    reuseTabPage(dt, tabPageIndex);
                }
                else {
                    createNewTabPage(dt, tabPageIndex);
                }
                tabPageIndex++;
            }

            removeAnyExtraTabs(ds.Tables.Count);
        }

        #region non-public
        private void createNewTabPage(DataTable sourceDataTable, int tabPageIndex) {
            var newResultTab = new TabPage($"Result #{tabPageIndex + 1}");
            _tabContainer.TabPages.Add(newResultTab);
            var resultsPane = new ResultsPaneView(sourceDataTable)
                                  .WithDockStyle(DockStyle.Fill)
                                  .WithContainerIndex(tabPageIndex)
                                  .WithResultsCountChangedHandler(ResultsPane_OnResultsCountChanged);
            newResultTab.Controls.Add(resultsPane);
        }

        private void reuseTabPage(DataTable sourceDataTable, int tabPageIndex) {
            var resultsPaneView = getChildControl<ResultsPaneView>(_tabContainer.TabPages[tabPageIndex]);
            resultsPaneView?.SetDataSource(sourceDataTable);
        }

        private void removeAnyExtraTabs(int dataTableCount) {
            var extraTabs = _tabContainer.TabCount - dataTableCount;
            while (extraTabs > 0) {
                _tabContainer.TabPages.RemoveAt(_tabContainer.TabPages.Count - 1);
                extraTabs--;
            }
        }

        private T getChildControl<T>(Control parentControl) where T : class {
            T childControl = null;
            foreach (var control in parentControl.Controls) {
                if (control is T) {
                    return control as T;
                }
            }
            return childControl;
        }

        private void ResultsPane_OnResultsCountChanged(object sender, ResultsCountChangedArgs e) {
            var tabText = _tabContainer.TabPages[e.ContainerIndex].Text;
            var pipeCharIndex = tabText.IndexOf('|');
            var preText = pipeCharIndex > -1 ? tabText.Substring(0, pipeCharIndex).Trim() : tabText;
            _tabContainer.TabPages[e.ContainerIndex].Text = $"{preText} | rows: {e.NewCount}";
        }
        #endregion non-public
    }
}
