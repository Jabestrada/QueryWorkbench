using QueryWorkbenchUI.Orchestration;
using System;
using System.IO;
using System.Windows.Forms;

namespace QueryWorkbenchUI.UserControls {
    public class QWBTabPage : TabPage, IQueryWorkspaceContainer, IDirtyable {
        private QueryWorkspaceView _workspace;


        public QWBTabPage(string title) : base(title) {

        }

        #region IDirtyable
        public event EventHandler<DirtyChangedEventArgs> OnDirtyChanged;

        public bool IsDirty {
            get {
                return _workspace.IsDirty;
            }

            protected set {
                if (value) {
                    if (!Text.EndsWith("*")) {
                        Text += "*";
                    }
                }
                else {
                    if (Text.EndsWith("*")) {
                        Text = Text.Substring(0, base.Text.Length - 1).Trim();
                    }
                }
                OnDirtyChanged?.Invoke(this, new DirtyChangedEventArgs(value));
            }
        }

        #endregion IDirtyable

        public void SetWorkspace(QueryWorkspaceView workspace) {
            workspace.OnDirtyChanged += Workspace_OnDirtyChanged;
            workspace.OnSaved += Workspace_OnSaved;
            Controls.Add(workspace);
            _workspace = workspace;
        }

        private void Workspace_OnSaved(object sender, OnSavedEventArgs e) {
            WorkspaceTitle = Path.GetFileName(e.Filename);
        }

        private void Workspace_OnDirtyChanged(object sender, DirtyChangedEventArgs e) {
            IsDirty = e.IsDirty;
        }


        #region IQueryWorkspaceContainer
        public string WorkspaceTitle {
            get {
                if (base.Text.EndsWith("*")) {
                    return base.Text.Substring(0, base.Text.Length - 1).Trim();
                }
                return base.Text;
            }
            set {
                base.Text = value;
            }
        }

        #endregion IQueryWorkspaceContainer
    }
}

