using QueryWorkbenchUI.Models;
using QueryWorkbenchUI.Orchestration;
using System;

namespace QueryWorkbenchUI {
    public interface IQueryWorkspace : IDirtyable, ITabbedResultsView, IResultsView {
        event EventHandler<OnSavedEventArgs> OnSaved;

        Workspace Model { get; }
        void RunQuery();
        bool Save(IWorkspaceController workspaceController);
        bool Close(IWorkspaceController workspaceController, bool force);
        Workspace CloneModel();
        bool IsResultsPaneVisible { get; set; }
        bool IsParametersPaneVisible { get; set; }

        void CommentLine();
        void UncommentLine();

    }

    public class OnSavedEventArgs : EventArgs {
        public readonly string Filename;
        public OnSavedEventArgs(string filename) {
            Filename = filename;
        }
    }
}   
