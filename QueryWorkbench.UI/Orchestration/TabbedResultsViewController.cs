using QueryWorkbenchUI.Extensions;
using QueryWorkbenchUI.Models;
using QueryWorkbenchUI.UserControls;
using QueryWorkbenchUI.UserForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QueryWorkbenchUI.Orchestration {
    public class TabbedResultsViewController : IDirtyable, ITabbedResultsView, IResultsView {
        private readonly TabControl _tabContainer;
        private ContextMenu _tabPageContextMenu;
        private string[] _resultPaneTitles;

        private const char TAB_TITLE_SEPARATOR_CHAR = '|';

        #region IDirtyable
        public bool IsDirty { get; set; }

        public event EventHandler<DirtyChangedEventArgs> OnDirtyChanged;

        #endregion IDirtyable

        public TabbedResultsViewController(TabControl tabContainer) {
            _tabContainer = tabContainer;
            bindContextMenu();

        }

        public IEnumerable<string> GetResultTabTitles() {
            foreach (TabPage tabPage in _tabContainer.TabPages) {
                yield return getTabTitle(tabPage);
            }
        }

        #region IResultsView
        public event EventHandler<ResultsCountChangedArgs> OnResultsCountChanged;

        public void ApplyFilter() {
            ActiveResultsView?.ApplyFilter();
        }

        public bool IsOutputPaneVisible {
            get {
                return ActiveResultsView?.IsOutputPaneVisible == true;
            }
            set {
                if (ActiveResultsView != null) {
                    ActiveResultsView.IsOutputPaneVisible = value;
                }
            }
        }
        #endregion IResultsView

        #region ITabbedResultsView
        public void CycleResultsTabForward() {
            _tabContainer.SelectNextTab();
        }

        public void CycleResultsTabBackward() {
            _tabContainer.SelectPreviousTab();

        }
        #endregion ITabbedResultsView

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

        public void BindWorkspaceModel(Workspace workspaceModel) {
            _resultPaneTitles = workspaceModel.ResultPaneTitles.ToArray();
        }

        #region non-public
        private IResultsView ActiveResultsView {
            get {
                if (_tabContainer.SelectedTab == null) {
                    return null;
                }
                return getChildControl<IResultsView>(_tabContainer.SelectedTab);
            }

        }


        private void bindContextMenu() {
            _tabContainer.MouseUp += _tabContainer_MouseUp;

            _tabPageContextMenu = new ContextMenu();
            var renameTabMenuItem = new MenuItem("Rename tab", new EventHandler(renameTabHandler));
            _tabPageContextMenu.MenuItems.Add(renameTabMenuItem);
        }

        private void _tabContainer_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                for (int i = 0; i < _tabContainer.TabCount; ++i) {
                    Rectangle r = _tabContainer.GetTabRect(i);
                    if (r.Contains(e.Location)) {
                        _tabContainer.SelectedIndex = i;
                        _tabPageContextMenu.Show(_tabContainer, e.Location);
                        break;
                    }
                }
            }
        }

        private void renameTabHandler(object sender, EventArgs e) {
            var textInputDialog = new TextInputDialog("Results tab text", getTabTitle(_tabContainer.SelectedTab));
            if (textInputDialog.ShowDialog() == DialogResult.OK) {
                setTabTitle(_tabContainer.SelectedTab, textInputDialog.Input);
            }
        }

        private void createNewTabPage(DataTable sourceDataTable, int tabPageIndex) {
            var tabTitle = _resultPaneTitles != null && tabPageIndex < _resultPaneTitles.Length
                           ? _resultPaneTitles[tabPageIndex]
                           : $"Result #{tabPageIndex + 1}";
            var newResultTab = new TabPage(tabTitle);
            _tabContainer.TabPages.Add(newResultTab);

            var resultsPane = new ResultsPaneView(tabTitle, sourceDataTable)
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
            var preText = getTabTitle(_tabContainer.TabPages[e.ContainerIndex]);
            var rowsText = e.NewCount > 1 ? "rows" : "row";
            _tabContainer.TabPages[e.ContainerIndex]
                         .Text = $"{preText} {TAB_TITLE_SEPARATOR_CHAR} {e.NewCount} {rowsText}";

        }

        private void setTabTitle(TabPage tabPage, string title) {
            var currentTabTitle = getTabTitle(tabPage);
            if (currentTabTitle == title.Trim()) {
                return;
            }
            var currentTabTitleRaw = tabPage.Text;
            var pipeCharIndex = currentTabTitleRaw.IndexOf(TAB_TITLE_SEPARATOR_CHAR);
            tabPage.Text = pipeCharIndex < 0 ?
                           title :
                           $"{title} {tabPage.Text.Substring(pipeCharIndex)}";
            OnDirtyChanged(this, new DirtyChangedEventArgs(true));
        }

        private string getTabTitle(TabPage tabPage) {
            var tabText = tabPage.Text;
            var pipeCharIndex = tabText.IndexOf(TAB_TITLE_SEPARATOR_CHAR);
            return pipeCharIndex > -1 ? tabText.Substring(0, pipeCharIndex).Trim() : tabText;
        }




        #endregion non-public
    }
}
