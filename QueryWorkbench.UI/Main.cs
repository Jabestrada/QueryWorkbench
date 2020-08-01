using QueryWorkbenchUI;
using QueryWorkbenchUI.ApplicationState;
using QueryWorkbenchUI.Configuration;
using QueryWorkbenchUI.Dialogs;
using QueryWorkbenchUI.Extensions;
using QueryWorkbenchUI.Orchestration;
using QueryWorkbenchUI.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QueryWorkBench.UI {
    public partial class Main : Form, IWorkspaceController {
        private KeyboardShortcutsProvider _keybShortcutsProvider = new KeyboardShortcutsProvider();

        private Dictionary<Keys, Action> _keybShortcutsMap = new Dictionary<Keys, Action>();

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

        public bool AcceptKeys(Keys keyData) {
            if (_keybShortcutsMap.ContainsKey(keyData)) {
                _keybShortcutsMap[keyData].Invoke();
                refreshUIState();
                return true;
            }
            return false;
        }


        #region overrides
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (!AcceptKeys(keyData)) {
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
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.E, Keys.None), runQuery, "Run Query");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.F, Keys.None), applyFilter, "Apply Filter");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.N, Keys.None), newWorkspace, "New Workspace");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.S, Keys.None), saveWorkspace, "Save Workspace");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.O, Keys.None), openWorkspace, "Open Workspace");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.Q, Keys.None), forcedCloseWorkspace, "Forced Close Workspace");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.W, Keys.None), closeWorkspace, "Close Workspace");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.D, Keys.None), cloneWorkspace, "Clone Workspace");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.T, Keys.None), cycleWorkspaceTabsForward, "Select Next Workspace Tab");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.Shift, Keys.T), cycleWorkspaceTabsBackward, "Select Previous Workspace Tab");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.R, Keys.None), toggleResultsPane, "Toggle Results Pane");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.P, Keys.None), toggleParametersPane, "Toggle Parameters Pane");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.Shift, Keys.O), toggleOutputPane, "Toggle Output Pane");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.M, Keys.None), cycleResultsTabsForward, "Select Next Resuls Tab");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.Shift, Keys.M), cycleResultsTabsBackward, "Select Previous Resuls Tab");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.Shift, Keys.K), commentLine, "Comment Line (TODO)");
            _keybShortcutsProvider.Add(Tuple.Create(Keys.Control, Keys.Shift, Keys.U), uncommentLine, "Uncomment Line (TODO)");

            _keybShortcutsMap = _keybShortcutsProvider.GetKeyboardActionsMap();
        }

        private void uncommentLine() {
            ActiveQueryWorkspace?.UncommentLine();
        }

        private void commentLine() {
            ActiveQueryWorkspace?.CommentLine();
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

        private void cycleResultsTabsForward() {
            ActiveQueryWorkspace?.CycleResultsTabForward();
        }

        private void cycleResultsTabsBackward() {
            ActiveQueryWorkspace?.CycleResultsTabBackward();
        }

        private void toggleOutputPane() {
            if (ActiveQueryWorkspace == null) return;

            ActiveQueryWorkspace.IsOutputPaneVisible = !ActiveQueryWorkspace.IsOutputPaneVisible;
        }


        private void toggleParametersPane() {
            if (ActiveQueryWorkspace == null) return;

            var current = ActiveQueryWorkspace.IsParametersPaneVisible;
            ActiveQueryWorkspace.IsParametersPaneVisible = !current;
        }

        private void toggleResultsPane() {
            if (ActiveQueryWorkspace == null) return;

            ActiveQueryWorkspace.IsResultsPaneVisible = !ActiveQueryWorkspace.IsResultsPaneVisible;
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

        private void shortcutsToolStripMenuItem_Click(object sender, EventArgs e) {
            var keysDescriptionMap = _keybShortcutsProvider.GetKeyboardDescriptionMap();
            StringBuilder allDescriptionsString = new StringBuilder();
            foreach (var descriptionMap in keysDescriptionMap) {
                allDescriptionsString.Append(descriptionMap.Key);
                allDescriptionsString.Append("\t");
                if (descriptionMap.Value.KeyCount == 2) {
                    allDescriptionsString.Append("\t");
                }
                allDescriptionsString.Append(descriptionMap.Value.Description);
                allDescriptionsString.Append(Environment.NewLine);
            }
            MessageBox.Show(allDescriptionsString.ToString(), "Keyboard Shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void cycleWorkspaceTabsForward() {
            mainTabControl.SelectNextTab();
        }

        private void cycleWorkspaceTabsBackward() {
            mainTabControl.SelectPreviousTab();
        }
        #endregion

    }
}
