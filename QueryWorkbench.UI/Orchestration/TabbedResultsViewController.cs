using QueryWorkbenchUI.UserControls;
using System.Data;
using System.Windows.Forms;

namespace QueryWorkbenchUI.Orchestration {
    public class TabbedResultsViewController {
        private readonly TabControl _tabContainer;
        public TabbedResultsViewController(TabControl tabContainer) {
            _tabContainer = tabContainer;
        }

        public void BindResults(DataSet ds) {
            // TODO: Don't always recreate tab pages and ResultsPaneView so that Filters
            // are retained if user mistakenly executes query rather than apply filters.
            _tabContainer.TabPages.Clear();
            var counter = 1;
            foreach (DataTable dt in ds.Tables) {
                var newResultTab = new TabPage($"Result #{counter}");
                _tabContainer.TabPages.Add(newResultTab);
                var resultsPane = new ResultsPaneView(dt)
                                      .WithDockStyle(DockStyle.Fill)
                                      .WithContainerIndex(counter - 1)
                                      .WithResultsCountChangedHandler(ResultsPane_OnResultsCountChanged);
                newResultTab.Controls.Add(resultsPane);
                counter++;
            }
        }

        public void ApplyFilter() {
            if (_tabContainer.SelectedTab == null) {
                return;
            }
            IResultsView selectedResultsView = getResultsPanelView(_tabContainer.SelectedTab);

            if (selectedResultsView != null) {
                selectedResultsView.ApplyFilter();
            }
        }

        private IResultsView getResultsPanelView(TabPage selectedTab) {
            if (selectedTab == null) {
                return null;
            }

            foreach (Control childControl in selectedTab.Controls) {
                if (childControl is IResultsView) {
                    return childControl as IResultsView;
                }
            }
            return null;
        }

        private void ResultsPane_OnResultsCountChanged(object sender, ResultsCountChangedArgs e) {
            var tabText = _tabContainer.TabPages[e.ContainerIndex].Text;
            var pipeCharIndex = tabText.IndexOf('|');
            var preText = pipeCharIndex > -1 ? tabText.Substring(0, pipeCharIndex).Trim() : tabText;
            _tabContainer.TabPages[e.ContainerIndex].Text = $"{preText} | rows: {e.NewCount}";
        }
    }
}
