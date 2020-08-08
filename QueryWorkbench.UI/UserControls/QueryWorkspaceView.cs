using QueryWorkbench.Infrastructure;
using QueryWorkbench.SqlServer;
using QueryWorkbenchUI.Models;
using QueryWorkbenchUI.Orchestration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace QueryWorkbenchUI.UserControls {
    public partial class QueryWorkspaceView : UserControl, IQueryWorkspace, IDirtyable {

        private string _filename;
        private TabbedResultsViewController _resultsViewController;
        private IQueryWorkspaceContainer _workspaceContainer;
        private IDbCommandDispatcher _sqlCommandDispatcher;
        private bool _runQueryAsync;
        private RichTextBox _statusTextbox;

        delegate void PostQueryCallback(DataSet data, Exception exc);

        public Workspace Model {
            get {
                return buildWorkspaceModel();
            }
        }

        public IDbCommandDispatcher SqlCommandDispatcher {
            get {
                if (_sqlCommandDispatcher == null) {
                    // TODO: Use Factory to create BaseCommandDispatcher type.
                    _sqlCommandDispatcher = new SqlServerCommandDispatcher(txtConnString.Text);
                }
                return _sqlCommandDispatcher;
            }

            protected set {
                _sqlCommandDispatcher = value;
            }
        }
        #region ctors
        public QueryWorkspaceView(IDbCommandDispatcher sqlCommandDispatcher) : this() {
            SqlCommandDispatcher = sqlCommandDispatcher;
        }

        public QueryWorkspaceView() {
            InitializeComponent();
            initializeResultsViewControler();
            initializeStatusTextbox();

            IsDirty = true;
            _runQueryAsync = true;
        }

        public QueryWorkspaceView(Workspace workspaceModel) : this() {
            bindWorkspaceModel(workspaceModel);

        }

        public QueryWorkspaceView(string filename) : this() {
            _filename = filename;
            var serializer = new XmlSerializer(typeof(Workspace));
            using (var fileStream = new FileStream(filename, FileMode.Open)) {
                Workspace workspace = (Workspace)serializer.Deserialize(fileStream);
                bindWorkspaceModel(workspace);
            }
            IsDirty = false;
        }

        #endregion ctors

        #region IQueryWorkspace

        #region inherited from IResultsView
        public event EventHandler<ResultsCountChangedArgs> OnResultsCountChanged;

        public virtual void ApplyFilter() {
            _resultsViewController.ApplyFilter();
        }

        public bool IsOutputPaneVisible {
            get {
                return _resultsViewController.IsOutputPaneVisible;
            }
            set {
                _resultsViewController.IsOutputPaneVisible = value;
            }
        }

        public void CycleResultsTabForward() {
            _resultsViewController.CycleResultsTabForward();
        }

        public void CycleResultsTabBackward() {
            _resultsViewController.CycleResultsTabBackward();
        }

        #endregion inherited from IResultsView

        private WorkspaceState _state;

        public WorkspaceState State {
            get {
                return _state;
            }
            protected set {
                _state = value;
                var isReady = _state.IsReady();
                txtConnString.Enabled = isReady;
                txtQuery.Enabled = isReady;
                txtParams.Enabled = isReady;
                _statusTextbox.Visible = _state == WorkspaceState.Busy ||
                                         _state == WorkspaceState.ReadyWithError;
                _resultsViewController.Visible = _state == WorkspaceState.Ready;

                if (!isReady) {
                    mainSplitContainer.Panel2Collapsed = false;
                }
            }
        }

        public virtual bool Save(IWorkspaceController workspaceController) {
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
            RaiseOnSavedEvent(_filename);
            return true;
        }

        public virtual void RunQuery() {
            if (State == WorkspaceState.Busy) return;

            State = WorkspaceState.Busy;

            var sql = getSQL();
            var queryParams = getQueryParams();
            _statusTextbox.Text = "Running query ...";
            if (_runQueryAsync) {
                ThreadStart starter = new ThreadStart(() => {
                    runQueryInternal(sql, queryParams);
                });
                new Thread(starter).Start();
            }
            else {
                runQueryInternal(sql, queryParams);
            }
        }

        public virtual bool Close(IWorkspaceController workspaceController, bool force) {
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

        public bool IsResultsPaneVisible {
            get {
                return !mainSplitContainer.Panel2Collapsed;
            }
            set {
                if (!_state.IsReady()) return;

                mainSplitContainer.Panel2Collapsed = !value;
            }
        }

        public bool IsParametersPaneVisible {
            get {
                return !queryAndParametersContainer.Panel2Collapsed;
            }
            set {
                if (!_state.IsReady()) return;

                queryAndParametersContainer.Panel2Collapsed = !value;
            }
        }

        public void CommentLine() {
            var caretPosAfterUpdate = txtQuery.SelectionStart + SqlCommandDispatcher.LineCommentToken.Length;
            var firstCharIndexOfCurrentLine = txtQuery.GetFirstCharIndexOfCurrentLine();
            var currentLine = txtQuery.GetLineFromCharIndex(firstCharIndexOfCurrentLine);
            txtQuery.Text = txtQuery.Text.Insert(firstCharIndexOfCurrentLine, SqlCommandDispatcher.LineCommentToken);

            // TODO: Decide if syntax highlighting will be supported; below is for comment
            //var currentLineText = txtQuery.Lines[currentLine];
            //txtQuery.Select(firstCharIndexOfCurrentLine, currentLineText.Length);
            //txtQuery.SelectionColor = Color.Green;

            txtQuery.SelectionStart = caretPosAfterUpdate;
            txtQuery.SelectionLength = 0;
        }

        public void UncommentLine() {
            var lineCommentTokenLength = SqlCommandDispatcher.LineCommentToken.Length;
            var caretPosAfterUpdate = txtQuery.SelectionStart - lineCommentTokenLength;
            var firstCharIndexOfCurrentLine = txtQuery.GetFirstCharIndexOfCurrentLine();

            if ((firstCharIndexOfCurrentLine + lineCommentTokenLength - 1) >= txtQuery.Text.Length) return;

            var queryText = txtQuery.Text;
            string substringFromCurrentIndex = queryText[firstCharIndexOfCurrentLine].ToString() +
                                               queryText[firstCharIndexOfCurrentLine + (lineCommentTokenLength - 1)].ToString();
            if (substringFromCurrentIndex != SqlCommandDispatcher.LineCommentToken) return;

            txtQuery.Text = queryText.Substring(0, firstCharIndexOfCurrentLine) +
                            queryText.Substring(firstCharIndexOfCurrentLine + lineCommentTokenLength);
            txtQuery.SelectionStart = caretPosAfterUpdate;
        }
        #endregion IQueryWorkspace

        #region IDirtyable
        public event EventHandler<DirtyChangedEventArgs> OnDirtyChanged;
        public event EventHandler<OnSavedEventArgs> OnSaved;

        public bool IsDirty { get; protected set; }

        #endregion

        protected virtual void RaiseOnSavedEvent(string filename) {
            OnSaved?.Invoke(this, new OnSavedEventArgs(filename));
        }

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
        public QueryWorkspaceView WithRunQueryAsync(bool runQueryAsync) {
            _runQueryAsync = runQueryAsync;
            return this;
        }
        #endregion

        #region query execution
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

        private void runQueryInternal(string sql, Dictionary<string, object> queryParams) {
            DataSet data = null;
            try {
                data = SqlCommandDispatcher.RunQuery(sql, queryParams);
                if (InvokeRequired) {
                    Invoke(new PostQueryCallback(onPostQuery), new object[] { data, null });
                }
                else {
                    onPostQuery(data, null);
                }
            }
            catch (Exception exc) {
                if (InvokeRequired) {
                    Invoke(new PostQueryCallback(onPostQuery), new object[] { null, exc });
                }
                else {
                    onPostQuery(null, exc);
                }
            }
        }
        private void onPostQuery(DataSet data, Exception exc) {
            if (exc == null) {
                _resultsViewController.BindResults(data);
                mainSplitContainer.Panel2Collapsed = false;
                State = WorkspaceState.Ready;
            }
            else {
                _statusTextbox.Text = $"Error running query: {exc.Message}";
                _statusTextbox.SelectAll();
                _statusTextbox.SelectionColor = Color.Red;
                State = WorkspaceState.ReadyWithError;
            }
        }

        #endregion query execution

        #region Misc non-public
        private void initializeResultsViewControler() {
            _resultsViewController = new TabbedResultsViewController(resultsTab);
            _resultsViewController.OnDirtyChanged += resultsViewController_OnDirtyChanged;
        }

        private void initializeStatusTextbox() {
            _statusTextbox = new RichTextBox();
            _statusTextbox.Dock = DockStyle.Fill;
            _statusTextbox.ReadOnly = true;
            _statusTextbox.Visible = false;
            mainSplitContainer.Panel2.Controls.Add(_statusTextbox);
        }

        private Workspace buildWorkspaceModel() {
            var workspace = new Workspace {
                Parameters = txtParams.Text,
                ConnectionString = txtConnString.Text,
                Query = txtQuery.Text
            };
            foreach (var title in _resultsViewController.GetResultTabTitles()) {
                workspace.ResultPaneTitles.Add(title);
            }
            return workspace;
        }

        private void bindWorkspaceModel(Workspace workspaceModel) {
            txtConnString.Text = workspaceModel.ConnectionString;
            txtQuery.Text = workspaceModel.Query?.Replace("\n", Environment.NewLine);
            txtParams.Text = workspaceModel.Parameters?.Replace("\n", Environment.NewLine);
            _resultsViewController.BindWorkspaceModel(workspaceModel);
        }

        #endregion Misc non-public

        #region Control event handlers
        private void txtQuery_TextChanged(object sender, EventArgs e) {
            onDirty(true);
        }

        private void txtParams_TextChanged(object sender, EventArgs e) {
            onDirty(true);
        }
        private void txtConnString_TextChanged(object sender, EventArgs e) {
            onDirty(true);

        }

        private void resultsViewController_OnDirtyChanged(object sender, DirtyChangedEventArgs e) {
            onDirty(e.IsDirty);
        }

        private void onDirty(bool isDirty) {
            IsDirty = isDirty;
            OnDirtyChanged?.Invoke(this, new DirtyChangedEventArgs(isDirty));
        }


        #endregion


    }
}
