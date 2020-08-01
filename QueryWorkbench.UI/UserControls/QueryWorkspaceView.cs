using QueryWorkbench.Infrastructure;
using QueryWorkbench.SqlServer;
using QueryWorkbenchUI.Models;
using QueryWorkbenchUI.Orchestration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace QueryWorkbenchUI.UserControls {
    public partial class QueryWorkspaceView : UserControl, IQueryWorkspace, IDirtyable {

        private string _filename;
        private readonly TabbedResultsViewController _resultsViewController;
        private IQueryWorkspaceContainer _workspaceContainer;
        private IDbCommandDispatcher _sqlCommandDispatcher;

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

            IsDirty = true;
            _resultsViewController = new TabbedResultsViewController(resultsTab);
            _resultsViewController.OnDirtyChanged += resultsViewController_OnDirtyChanged;
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
            //OnSaved?.Invoke(this, new OnSavedEventArgs(_filename));
            RaiseOnSavedEvent(_filename);
            return true;
        }

        public virtual void RunQuery() {
            var data = SqlCommandDispatcher.RunQuery(getSQL(), getQueryParams());
            _resultsViewController.BindResults(data);
            mainSplitContainer.Panel2Collapsed = false;
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
                mainSplitContainer.Panel2Collapsed = !value;
            }
        }

        public bool IsParametersPaneVisible {
            get {
                return !queryAndParametersContainer.Panel2Collapsed;
            }
            set {
                queryAndParametersContainer.Panel2Collapsed = !value;
            }
        }

        public void CommentLine() {
            var caretPosAfterUpdate = txtQuery.SelectionStart + SqlCommandDispatcher.LineCommentToken.Length;
            var firstCharIndexOfCurrentLine = txtQuery.GetFirstCharIndexOfCurrentLine();
            txtQuery.Text = txtQuery.Text.Insert(firstCharIndexOfCurrentLine, SqlCommandDispatcher.LineCommentToken);
            txtQuery.SelectionStart = caretPosAfterUpdate;
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
        #endregion

        #region Misc non-public
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
