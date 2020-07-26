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
        void ToggleResultsPane();
        void ToggleParametersPane();
    }

    public class OnSavedEventArgs : EventArgs {
        public readonly string Filename;
        public OnSavedEventArgs(string filename) {
            Filename = filename;
        }
    }
}   
