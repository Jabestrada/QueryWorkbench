namespace QueryWorkbenchUI.Orchestration {
    public interface IWorkspaceController {
        void RequestNewFile(RequestNewFileArgs args);
    }

    public class RequestNewFileArgs {
        public bool Cancel { get; set; }

        public string Filename { get; set; }

        public RequestNewFileArgs() : this(false, null) {

        }

        public RequestNewFileArgs(bool cancel, string filename) {
            Cancel = cancel;
            Filename = filename;
        }
    }
}