using QueryWorkbenchUI.Dialogs;
using System.Windows.Forms;

namespace QueryWorkbench.Tests.Mocks {
    public class MockOpenFileDialog : IOpenFileDialog {
        public bool NewDialogShown { get; protected set; }
        public bool OpenDialogShown { get; protected set; }

        public MockOpenFileDialog(string filename = "") {
            FileName = filename;
        }
        public string FileName { get; protected set; }

        public DialogResult ShowNewWorkspaceDialog() {
            NewDialogShown = true;
            return DialogResult.OK;
        }

        public DialogResult ShowOpenWorkspaceDialog() {
            OpenDialogShown = true;
            return DialogResult.Cancel;
        }
    }
}
