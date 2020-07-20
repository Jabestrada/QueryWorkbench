using QueryWorkbenchUI.Models;
using QueryWorkbenchUI.Orchestration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace QueryWorkbenchUI.UserControls {
    public partial class QueryWorkspaceView : UserControl, IQueryWorkspace, IDirtyable {

        private string _filename;

        private IResultsView _selectedResultsView;

        private IQueryWorkspaceContainer _workspaceContainer;

        #region ctors
        public QueryWorkspaceView() {
            InitializeComponent();
            IsDirty = true;
        }

        public QueryWorkspaceView(Workspace workspaceModel) : this() {
            bindModel(workspaceModel);

        }


        public QueryWorkspaceView(string filename) : this() {
            _filename = filename;
            var serializer = new XmlSerializer(typeof(Workspace));
            using (var fileStream = new FileStream(filename, FileMode.Open)) {
                Workspace workspace = (Workspace)serializer.Deserialize(fileStream);
                bindModel(workspace);
            }
            IsDirty = false;
        }

        #endregion ctors

        #region IQueryWorkspace
        public void ApplyFilter() {
            if (resultsTab.SelectedTab == null) {
                return;
            }
            _selectedResultsView = getResultsPanelView(resultsTab.SelectedTab);

            if (_selectedResultsView != null) {
                _selectedResultsView.ApplyFilter();
            }
        }

        public bool Save(IWorkspaceController workspaceController) {
            if (string.IsNullOrWhiteSpace(_filename)) {
                var requestWriterArgs = new RequestNewFileArgs();
                workspaceController.RequestNewFile(requestWriterArgs);
                if (requestWriterArgs.Cancel) {
                    return false;
                }
                if (requestWriterArgs.Filename == null) {
                    throw new ApplicationException("Filename returned by Controller is null");
                }
                _filename = requestWriterArgs.Filename;
            }

            var workspaceModel = buildWorkspaceModel();
            XmlSerializer serializer = new XmlSerializer(typeof(Workspace));
            using (StreamWriter sw = new StreamWriter(_filename)) {
                serializer.Serialize(sw, workspaceModel);
                sw.Close();
            }
            IsDirty = false;
            OnDirtyChanged?.Invoke(this, new DirtyChangedEventArgs(false));
            OnSaved?.Invoke(this, new OnSavedEventArgs(_filename));
            return true;
        }

        public void RunQuery() {
            runQuery();
        }

        public bool Close(IWorkspaceController workspaceController, bool force) {
            if (force || !IsDirty) {
                return true;
            }

            var userResponse = MessageBox.Show($"Save changes to {_workspaceContainer?.WorkspaceTitle ?? Path.GetFileName(_filename)}?",
                               "Confirmation",
                               MessageBoxButtons.YesNoCancel,
                               MessageBoxIcon.Question);
            if (userResponse == DialogResult.No) {
                return true;
            }
            else if (userResponse == DialogResult.Cancel) {
                return false;
            }
            else {
                return Save(workspaceController);
            }
        }

        public Workspace CloneModel() {
            return buildWorkspaceModel();
        }

        public void ToggleResultsPane() {
            mainSplitContainer.Panel2Collapsed = !mainSplitContainer.Panel2Collapsed;
        }
        public void ToggleParametersPane() {
            queryAndParametersContainer.Panel2Collapsed = !queryAndParametersContainer.Panel2Collapsed; 
        }
        #endregion IQueryWorkspace

        #region IDirtyable
        public event EventHandler<DirtyChangedEventArgs> OnDirtyChanged;
        public event EventHandler<OnSavedEventArgs> OnSaved;

        public bool IsDirty { get; protected set; }
        #endregion

        #region Fluent builder methods
        public static QueryWorkspaceView New() {
            return new QueryWorkspaceView();
        }
        public static QueryWorkspaceView New(Workspace workspace) {
            return new QueryWorkspaceView(workspace);
        }
        public static QueryWorkspaceView New(string filename) {
            return new QueryWorkspaceView(filename);
        }
        public QueryWorkspaceView WithDockStyle(DockStyle dockStyle) {
            Dock = dockStyle;
            return this;
        }

        public QueryWorkspaceView WithContainer(IQueryWorkspaceContainer container) {
            _workspaceContainer = container;
            return this;
        }
        #endregion

        #region Misc non-public
        private void runQuery() {
            using (var sqlConn = new SqlConnection(txtConnString.Text)) {
                var sqlText = getSQL();
                var sqlDataAdapter = new SqlDataAdapter(sqlText, sqlConn);
                var queryParams = getQueryParams();
                foreach (var qp in queryParams) {
                    var paramName = qp.Key;
                    if (!paramName.StartsWith("@")) {
                        paramName = "@" + paramName;
                    }
                    sqlDataAdapter.SelectCommand.Parameters.AddWithValue(paramName, qp.Value);
                }

                var ds = new DataSet();
                sqlDataAdapter.Fill(ds);
                bindResults(ds);
                mainSplitContainer.Panel2Collapsed = false;
            }
        }

        private void bindResults(DataSet ds) {
            resultsTab.TabPages.Clear();
            var counter = 1;
            foreach (DataTable dt in ds.Tables) {
                var newResultTab = new TabPage($"Result #{counter}");
                resultsTab.TabPages.Add(newResultTab);
                var resultsPane = new ResultsPaneView(dt)
                                      .WithDockStyle(DockStyle.Fill)
                                      .WithContainerIndex(counter - 1)
                                      .WithResultsCountChangedHandler(ResultsPane_OnResultsCountChanged);
                newResultTab.Controls.Add(resultsPane);
                counter++;
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

        private string getSQL() {
            return string.IsNullOrWhiteSpace(txtQuery.SelectedText) ? txtQuery.Text : txtQuery.SelectedText;
        }

        private Dictionary<string, object> getQueryParams() {
            var paramsDictionary = new Dictionary<string, object>();
            var lines = txtParams.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                var paramPair = line.Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (paramPair.Length <= 0) {
                    continue;
                }
                paramsDictionary.Add(paramPair[0], paramPair[1]);
            }
            return paramsDictionary;
        }

        private Workspace buildWorkspaceModel() {
            var workspace = new Workspace {
                Parameters = txtParams.Text,
                ConnectionString = txtConnString.Text,
                Query = txtQuery.Text
            };
            return workspace;
        }

        private void bindModel(Workspace workspaceModel) {
            txtConnString.Text = workspaceModel.ConnectionString;
            txtQuery.Text = workspaceModel.Query.Replace("\n", Environment.NewLine);
            txtParams.Text = workspaceModel.Parameters.Replace("\n", Environment.NewLine);
        }

        #endregion Misc non-public

        #region Control event handlers
        private void ResultsPane_OnResultsCountChanged(object sender, ResultsCountChangedArgs e) {
            var tabText = resultsTab.TabPages[e.ContainerIndex].Text;
            var pipeCharIndex = tabText.IndexOf('|');
            var preText = pipeCharIndex > -1 ? tabText.Substring(0, pipeCharIndex).Trim() : tabText;
            resultsTab.TabPages[e.ContainerIndex].Text = $"{preText} | rows: {e.NewCount}";
        }

        private void txtQuery_TextChanged(object sender, EventArgs e) {
            IsDirty = true;
            OnDirtyChanged?.Invoke(this, new DirtyChangedEventArgs(true));
        }

        private void txtParams_TextChanged(object sender, EventArgs e) {
            IsDirty = true;
            OnDirtyChanged?.Invoke(this, new DirtyChangedEventArgs(true));
        }
        private void txtConnString_TextChanged(object sender, EventArgs e) {
            IsDirty = true;
            OnDirtyChanged?.Invoke(this, new DirtyChangedEventArgs(true));
        }

      

        #endregion


    }
}
