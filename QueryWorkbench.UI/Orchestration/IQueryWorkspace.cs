using QueryWorkbenchUI.Models;
using QueryWorkbenchUI.Orchestration;
using System;

namespace QueryWorkbenchUI {
    public interface IQueryWorkspace : IDirtyable {
        event EventHandler<OnSavedEventArgs> OnSaved;

        Workspace Model { get; }
        void RunQuery();
        void ApplyFilter();
        bool Save(IWorkspaceController workspaceController);
        bool Close(IWorkspaceController workspaceController, bool force);
        Workspace CloneModel();
        bool IsResultsPaneVisible { get; set; }
        bool IsParametersPaneVisible { get; set; }
        bool IsOutputPaneVisible { get; set; }
    }

    public class OnSavedEventArgs : EventArgs {
        public readonly string Filename;
        public OnSavedEventArgs(string filename) {
            Filename = filename;
        }
    }
}   
