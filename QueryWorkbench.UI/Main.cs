using QueryWorkbenchUI;
using QueryWorkbenchUI.ApplicationState;
using QueryWorkbenchUI.Configuration;
using QueryWorkbenchUI.Dialogs;
using QueryWorkbenchUI.Orchestration;
using QueryWorkbenchUI.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace QueryWorkBench.UI {
    public partial class Main : Form, IWorkspaceController {
        private Dictionary<Keys, Action> _kbShortcuts = new Dictionary<Keys, Action>();

        private AppState _appState = new AppState();
        private IOpenFileDialog _fileDialog = new QueryWorkbenchOpenFileDialog();
        private IAppStateStore _appStateStore = new FileSystemAppStateStore();

        public List<IQueryWorkspace> Workspaces { get; protected set; }

        public Main() {
            InitializeComponent();
            initializeKeyboardShortcuts();
            loadAppState();
            refreshUIState();

            Workspaces = new List<IQueryWorkspace>();
        }

        #region Fluent builders

        public Main WithOpenFileDialog(IOpenFileDialog openfileDialog) {
            _fileDialog = openfileDialog;
            return this;
        }

        public Main WithAppStateStore(IAppStateStore appStateStore) {
            _appStateStore = appStateStore;
            loadAppState();
            return this;
        }

        #endregion

        public ToolStripItemCollection MRUItems {
            get {
                return mrutoolStripMenuItem.DropDownItems;
            }
        }

        public void AddWorkspace(string title, QueryWorkspaceView workspaceView) {
            Workspaces.Add(workspaceView);
            var newTab = new QWBTabPage(title);
            var newWorkspace = workspaceView.WithDockStyle(DockStyle.Fill)
                                            .WithContainer(newTab);
            workspaceView.OnDirtyChanged += WorkspaceView_OnDirtyChanged;
            workspaceView.OnSaved += WorkspaceView_OnSaved;
            newTab.SetWorkspace(newWorkspace);
            mainTabControl.TabPages.Add(newTab);
            mainTabControl.SelectedTab = newTab;
        }

        public bool SendKeys(Keys keyData) {
            if (_kbShortcuts.ContainsKey(keyData)) {
                _kbShortcuts[keyData].Invoke();
                refreshUIState();
                return true;
            }
            return false;
        }


        #region overrides
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (!SendKeys(keyData)) {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            return true;
        }
        #endregion

        #region IWorkspaceController
        public void RequestNewFile(RequestNewFileArgs args) {
            if (_fileDialog.ShowNewWorkspaceDialog() == DialogResult.Cancel) {
                args.Cancel = true;
                args.Filename = null;
                return;
            }

            args.Cancel = false;
            args.Filename = _fileDialog.FileName;

        }
        #endregion

        #region New, Open, Clone workspace
        private void newWorkspace() {
            AddWorkspace($"Query Workspace {mainTabControl.TabPages.Count + 1} *", QueryWorkspaceView.New());
        }

        private void openWorkspace() {
            if (_fileDialog.ShowOpenWorkspaceDialog() == DialogResult.Cancel) {
                return;
            }
            openWorkspace(_fileDialog.FileName);
        }

        private void openWorkspace(string filename) {
            AddWorkspace(Path.GetFileName(filename), QueryWorkspaceView.New(filename));
            pushMRUItem(filename);
        }

        private void cloneWorkspace() {
            if (ActiveQueryWorkspace == null) {
                return;
            }
            var workspaceModel = ActiveQueryWorkspace.CloneModel();
            AddWorkspace($"Copy of {mainTabControl.SelectedTab.Text}", QueryWorkspaceView.New(workspaceModel));
        }
        #endregion new, open, clone workspace

        #region Close, Save workspace
        private void forcedCloseWorkspace() {
            closeWorkspace(true);
        }

        private void closeWorkspace() {
            closeWorkspace(false);
        }

        private void closeWorkspace(bool force) {
            if (mainTabControl.TabCount == 0) {
                Close();
            }
            Workspaces.Remove(ActiveQueryWorkspace);

            if (ActiveQueryWorkspace?.Close(this, force) == true) {
                mainTabControl.TabPages.Remove(mainTabControl.SelectedTab);
            }

        }

        private void saveWorkspace() {
            ActiveQueryWorkspace?.Save(this);
        }

        #endregion Close, Save workspace

        #region WorkspaceView event handlers
        private void WorkspaceView_OnSaved(object sender, OnSavedEventArgs e) {
            pushMRUItem(e.Filename);
        }

        private void WorkspaceView_OnDirtyChanged(object sender, DirtyChangedEventArgs e) {
            refreshUIState();
        }
        #endregion WorkspaceView event handlers

        #region Run Query, Apply Filter
        private void runQuery() {
            try {
                ActiveQueryWorkspace?.RunQuery();
            }
            catch (Exception exc) {
                MessageBox.Show(exc.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void applyFilter() {
            try {
                ActiveQueryWorkspace?.ApplyFilter();
            }
            catch (Exception exc) {
                MessageBox.Show(exc.Message);
            }
        }

        #endregion Run Query, Apply Filter

        #region Misc non-public
        private void initializeKeyboardShortcuts() {
            _kbShortcuts.Add(Keys.Control | Keys.E, runQuery);
            _kbShortcuts.Add(Keys.Control | Keys.F, applyFilter);
            _kbShortcuts.Add(Keys.Control | Keys.N, newWorkspace);
            _kbShortcuts.Add(Keys.Control | Keys.S, saveWorkspace);
            _kbShortcuts.Add(Keys.Control | Keys.O, openWorkspace);
            _kbShortcuts.Add(Keys.Control | Keys.Q, forcedCloseWorkspace);
            _kbShortcuts.Add(Keys.Control | Keys.W, closeWorkspace);
            _kbShortcuts.Add(Keys.Control | Keys.D, cloneWorkspace);
            _kbShortcuts.Add(Keys.Control | Keys.Shift | Keys.T, cycleWorkspaceTabsReverse);
            _kbShortcuts.Add(Keys.Control | Keys.T, cycleWorkspaceTabs);
            _kbShortcuts.Add(Keys.Control | Keys.R, toggleResultsPane);
            _kbShortcuts.Add(Keys.Control | Keys.P, toggleParametersPane);
            _kbShortcuts.Add(Keys.Control | Keys.Shift | Keys.O, toggleOutputPane);
            _kbShortcuts.Add(Keys.Control | Keys.M, cycleResultsTabs);
        }

        private void refreshUIState() {
            refreshMenuState();

        }

        private void refreshMenuState() {
            var hasActiveWorkspace = ActiveQueryWorkspace != null;
            saveWorkspaceToolStripMenuItem.Enabled = ActiveQueryWorkspace?.IsDirty == true;
            cloneWorkspaceStripMenuItem.Enabled = hasActiveWorkspace;
            closeWorkspaceToolStripMenuItem.Enabled = hasActiveWorkspace;
            mrutoolStripMenuItem.Enabled = _appState?.MRUConfigList?.Items?.Count > 0;
            closeWorkspaceWithoutSavingToolStripMenuItem.Enabled = hasActiveWorkspace;
        }

        private void cycleResultsTabs() {
            Debug.WriteLine("TODO: Cycle results tabs");
        }

        private void toggleOutputPane() {
            Debug.WriteLine("TODO: Toggle output pane");
        }


        private void toggleParametersPane() {
            ActiveQueryWorkspace?.ToggleParametersPane();
        }

        private void toggleResultsPane() {
            ActiveQueryWorkspace?.ToggleResultsPane();
        }

        public IQueryWorkspace ActiveQueryWorkspace {
            get {
                if (mainTabControl.TabCount == 0 || mainTabControl.SelectedTab == null) {
                    return null;
                }
                return getQueryWorkspaceReference(mainTabControl.SelectedTab);
            }
        }

        private IQueryWorkspace getQueryWorkspaceReference(Control control) {
            if (control == null) {
                return null;
            }
            if (control is IQueryWorkspace) {
                return control as IQueryWorkspace;
            }
            foreach (var childControl in control.Controls) {
                if (childControl is IQueryWorkspace) {
                    return childControl as IQueryWorkspace;
                }
            }
            return null;
        }
        #endregion Misc non-public

        #region Control event handlers
        private void toolStripNewTab_Click(object sender, EventArgs e) {
            newWorkspace();
        }
        private void toolstripSave_Click(object sender, EventArgs e) {
            saveWorkspace();
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason != CloseReason.UserClosing) {
                return;
            }

            foreach (TabPage tabPage in mainTabControl.TabPages) {
                mainTabControl.SelectedTab = tabPage;
                IQueryWorkspace workspace = getQueryWorkspaceReference(tabPage);
                var isWorkspaceClosed = workspace.Close(this, false);
                if (!isWorkspaceClosed) {
                    e.Cancel = true;
                    break;
                }
            }
        }

        #region Menu item events
        private void runToolStripMenuItem_Click(object sender, EventArgs e) {
            runQuery();
        }
        private void applyFilterToolStripMenuItem_Click(object sender, EventArgs e) {
            applyFilter();
        }

        private void openWorkspaceToolStripMenuItem_Click(object sender, EventArgs e) {
            openWorkspace();
        }

        private void newWorkspaceToolStripMenuItem_Click(object sender, EventArgs e) {
            newWorkspace();
        }

        private void saveWorkspaceToolStripMenuItem_Click(object sender, EventArgs e) {
            saveWorkspace();
        }

        private void closeWorkspaceToolStripMenuItem_Click(object sender, EventArgs e) {
            closeWorkspace();
        }

        private void closeWorkspaceWithoutSavingToolStripMenuItem1_Click(object sender, EventArgs e) {
            forcedCloseWorkspace();
        }
        #endregion Menu item events

        #endregion Control event handlers

        #region MRU, App state

        private string getAppStateFilename() {
            var appDir = Path.GetDirectoryName(Application.ExecutablePath);
            var datFile = $"{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}.dat";
            return Path.Combine(appDir, datFile);
        }

        private void pushMRUItem(string item) {
            if (_appState == null) {
                return;
            }
            _appState.MRUConfigList.AddItem(item);

            mrutoolStripMenuItem.DropDownItems.Clear();
            foreach (var mruItem in _appState.MRUConfigList.Items) {
                var menuItem = createMruMenuItem(mruItem);
                mrutoolStripMenuItem.DropDownItems.Add(menuItem);
            }

            if (!mrutoolStripMenuItem.Enabled) {
                mrutoolStripMenuItem.Enabled = true;
            }

            _appStateStore.SaveAppState(getAppStateFilename(), _appState);
        }

        private void loadAppState() {
            _appState = _appStateStore.LoadAppState(getAppStateFilename());
            if (_appState == null) {
                mrutoolStripMenuItem.Enabled = false;
                return;
            }

            foreach (var mruItem in _appState.MRUConfigList.Items) {
                var mruMenuItem = createMruMenuItem(mruItem);
                mrutoolStripMenuItem.DropDownItems.Add(mruMenuItem);
            }
        }

        private ToolStripMenuItem createMruMenuItem(KeyValuePair<int, string> i) {
            var menuItem = new ToolStripMenuItem($"{i.Key + 1} {i.Value}");
            menuItem.Tag = i.Value;
            menuItem.Click += mruItem_Click;
            return menuItem;
        }

        private void mruItem_Click(object sender, EventArgs e) {
            var mruItem = (ToolStripMenuItem)sender;
            var workspaceFile = mruItem.Tag.ToString();
            if (!setActiveTabIfLoaded(workspaceFile)) {
                openWorkspace(workspaceFile);
            }
        }




        #endregion MRU

        #region Tabs
        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e) {
            refreshUIState();
        }

        private bool setActiveTabIfLoaded(string workspaceFile) {
            var existingTab = getTabByFilename(workspaceFile);
            if (existingTab == null) {
                return false;
            }
            else {
                mainTabControl.SelectedTab = existingTab;
                return true;
            }
        }

        private TabPage getTabByFilename(string workspaceFile) {
            foreach (QWBTabPage tab in mainTabControl.TabPages) {
                if (tab.WorkspaceTitle.ToLower() == Path.GetFileName(workspaceFile).ToLower()) {
                    return tab;
                }
            }
            return null;
        }

        private void cycleWorkspaceTabs() {
            if (mainTabControl.TabPages == null || mainTabControl.TabPages.Count <= 1) {
                return;
            }
            mainTabControl.SelectedTab = mainTabControl.TabPages[(mainTabControl.SelectedIndex + 1) % mainTabControl.TabCount];
        }

        private void cycleWorkspaceTabsReverse() {
            if (mainTabControl.TabPages == null || mainTabControl.TabPages.Count <= 1) {
                return;
            }

            mainTabControl.SelectedTab = mainTabControl.TabPages[mainTabControl.SelectedIndex == 0 ? mainTabControl.TabCount - 1 : mainTabControl.SelectedIndex - 1];
        }
        #endregion

    }
}
