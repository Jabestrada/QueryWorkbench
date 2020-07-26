using QueryWorkbenchUI.Orchestration;
using QueryWorkbenchUI.UserControls;
using System.IO;

namespace QueryWorkbench.Tests.Mocks {
    public class MockQueryWorkspaceView : QueryWorkspaceView {

        public string Filename { get; protected set; }
        public bool WasClosed { get; protected set; }
        public bool DidApplyFilter { get; protected set; }
        public bool DidRunQuery { get; protected set; }
        public bool DidSaveWorkspace { get; protected set; }

        public MockQueryWorkspaceView() {

        }
        public MockQueryWorkspaceView(string filename) : base() {
            Filename = filename;
        }
        public MockQueryWorkspaceView(bool isDirty) : base() {
            IsDirty = isDirty;
        }

        public override bool Close(IWorkspaceController workspaceController, bool force) {
            WasClosed = force ||  !IsDirty;
            return WasClosed;
        }
        public override void ApplyFilter() {
            DidApplyFilter = true;
        }
        public override void RunQuery() {
            DidRunQuery = true;
        }

        public override bool Save(IWorkspaceController workspaceController) {
            DidSaveWorkspace = true;
            RaiseOnSavedEvent(Filename);
            return true;
        }
    }
}
