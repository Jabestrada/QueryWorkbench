using System.IO;
using System.Windows.Forms;

namespace QueryWorkbenchUI.Dialogs {
    public class QueryWorkbenchOpenFileDialog : IOpenFileDialog {
        public string FileName { get; protected set; }

        private const string FILE_NAME_EXTENSION = "qws";

        private string FileDialogFilter => $"Query Workspace Files | *.{FILE_NAME_EXTENSION}|All files (*.*)|*.*";
        
        public DialogResult ShowNewWorkspaceDialog() {
            var newFileDialog = new OpenFileDialog {
                CheckFileExists = false,
                Filter = FileDialogFilter,
                FileName = $"New Workspace File.{FILE_NAME_EXTENSION}"
            };
            var result = newFileDialog.ShowDialog();
            FileName = result == DialogResult.OK ? newFileDialog.FileName : null;
            return result;
        }

        public DialogResult ShowOpenWorkspaceDialog() {
            var openFileDialog = new OpenFileDialog {
                CheckFileExists = true,
                Filter = FileDialogFilter
            };
            var result = openFileDialog.ShowDialog();
            FileName = result == DialogResult.OK ? openFileDialog.FileName : null;
            return result;
        }
    }
}
