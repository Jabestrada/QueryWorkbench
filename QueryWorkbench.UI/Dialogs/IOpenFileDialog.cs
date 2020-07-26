using System.Windows.Forms;

namespace QueryWorkbenchUI.Dialogs {
    public interface IOpenFileDialog {
        string FileName { get; }
        DialogResult ShowNewWorkspaceDialog();
        DialogResult ShowOpenWorkspaceDialog();

    }
}
