using QueryWorkbenchUI;
using QueryWorkbenchUI.Configuration;
using QueryWorkbenchUI.Orchestration;
using QueryWorkbenchUI.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml;

namespace QueryWorkBench.UI {
    public partial class Main : Form, IWorkspaceController {
        private Dictionary<Keys, Action> _kbShortcuts = new Dictionary<Keys, Action>();
        private IQueryWorkspace _activeQueryWorkspace;

        private AppState _appState = new AppState();

        public Main() {
            InitializeComponent();
            initializeKeyboardShortcuts();
            loadAppState();
            //newTab();
        }

        #region overrides
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (_kbShortcuts.ContainsKey(keyData)) {
                _activeQueryWorkspace = getActiveQueryWorkspace();
                _kbShortcuts[keyData].Invoke();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region IWorkspaceController
        public void RequestNewFile(RequestNewFileArgs args) {
            var newFileDialog = new OpenFileDialog {
                CheckFileExists = false,
                Filter = "Query Workspace Files | *.qws|All files (*.*)|*.*",
                FileName = "New Workspace File.qws"
            };

            if (newFileDialog.ShowDialog() == DialogResult.Cancel) {
                args.Cancel = true;
                args.Filename = null;
                return;
            }

            args.Cancel = false;
            args.Filename = newFileDialog.FileName;
        }
        #endregion

        #region New, Open, Clone workspace
        private void newWorkspace() {
            createNewTab($"Query Workspace {mainTabControl.TabPages.Count + 1} *", QueryWorkspaceView.New());
        }

        private void openWorkspace() {
            var openFileDialog = new OpenFileDialog {
                CheckFileExists = true,
                Filter = "Query Workspace Files | *.qws|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == DialogResult.Cancel) {
                return;
            }
            openWorkspace(openFileDialog.FileName);
        }

        private void openWorkspace(string filename) {
            createNewTab(Path.GetFileName(filename), QueryWorkspaceView.New(filename));
            pushMRUItem(filename);
        }

        private void cloneWorkspace() {
            if (_activeQueryWorkspace == null) {
                return;
            }
            var workspaceModel = _activeQueryWorkspace.CloneModel();
            createNewTab($"Copy of {mainTabControl.SelectedTab.Text}", QueryWorkspaceView.New(workspaceModel));
        }
        private void createNewTab(string title, QueryWorkspaceView workspaceView) {
            var newTab = new QWBTabPage(title);
            var newWorkspace = workspaceView.WithDockStyle(DockStyle.Fill)
                                            .WithContainer(newTab);
            newTab.SetWorkspace(newWorkspace);
            mainTabControl.TabPages.Add(newTab);
            mainTabControl.SelectedTab = newTab;
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
            if (_activeQueryWorkspace == null) {
                return;
            }
            if (_activeQueryWorkspace.Close(this, force)) {
                mainTabControl.TabPages.Remove(mainTabControl.SelectedTab);
            }
        }

        private void saveWorkspace() {
            getActiveQueryWorkspace()?.Save(this);
        }

        #endregion close workspace

        #region Run Query, Apply Filter
        private void runQuery() {
            try {
                getActiveQueryWorkspace()?.RunQuery();
            }
            catch (Exception exc) {
                MessageBox.Show(exc.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void applyFilter() {
            try {
                getActiveQueryWorkspace()?.ApplyFilter();
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

        private void cycleResultsTabs() {
            Debug.WriteLine("TODO: Cycle results tabs");
        }

        private void toggleOutputPane() {
            Debug.WriteLine("TODO: Toggle output pane");
        }


        private void toggleParametersPane() {
            getActiveQueryWorkspace()?.ToggleParametersPane();
        }

        private void toggleResultsPane() {
            getActiveQueryWorkspace()?.ToggleResultsPane();
        }

        private IQueryWorkspace getActiveQueryWorkspace() {
            if (mainTabControl.SelectedTab == null) {
                return null;
            }
            return getQueryWorkspaceReference(mainTabControl.SelectedTab);
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
            _activeQueryWorkspace = getActiveQueryWorkspace();
            saveWorkspace();
        }

        private void closeWorkspaceToolStripMenuItem_Click(object sender, EventArgs e) {
            _activeQueryWorkspace = getActiveQueryWorkspace();
            closeWorkspace();
        }

        private void closeWorkspaceWithoutSavingToolStripMenuItem1_Click(object sender, EventArgs e) {
            _activeQueryWorkspace = getActiveQueryWorkspace();
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
            _appState.MRUConfigList.AddItem(item);

            mrutoolStripMenuItem.DropDownItems.Clear();
            foreach (var mruItem in _appState.MRUConfigList.Items) {
                var menuItem = createMruMenuItem(mruItem);
                mrutoolStripMenuItem.DropDownItems.Add(menuItem);
            }

            if (!mrutoolStripMenuItem.Enabled) {
                mrutoolStripMenuItem.Enabled = true;
            }

            saveAppState();
        }

        private void saveAppState() {
            var serializer = new DataContractSerializer(typeof(AppState));
            string xmlString;
            using (var sw = new StringWriter())
            using (var writer = new XmlTextWriter(sw)) {
                writer.Formatting = Formatting.Indented;
                serializer.WriteObject(writer, _appState);
                writer.Flush();
                xmlString = sw.ToString();
            }

            string appStateFile = getAppStateFilename();
            using (StreamWriter sw = new StreamWriter(appStateFile)) {
                sw.Write(xmlString);
                sw.Flush();
                sw.Close();
            }
        }

        private void loadAppState() {
            string appStateFile = getAppStateFilename();
            if (!File.Exists(appStateFile)) {
                mrutoolStripMenuItem.Enabled = false;
                return;
            }
            using (StreamReader sw = new StreamReader(appStateFile)) {
                var reader = new XmlTextReader(sw);
                var deserializer = new DataContractSerializer(typeof(AppState));
                var result = deserializer.ReadObject(reader);
                _appState = (AppState)result;

            }
            foreach (var mruItem in _appState.MRUConfigList.Items) {
                var mruMenuItem = createMruMenuItem(mruItem);
                mrutoolStripMenuItem.DropDownItems.Add(mruMenuItem);
            }
            mrutoolStripMenuItem.Enabled = _appState.MRUConfigList.Items.Count > 0;
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
