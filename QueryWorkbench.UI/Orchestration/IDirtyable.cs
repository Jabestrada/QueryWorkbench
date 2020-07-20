using System;

namespace QueryWorkbenchUI.Orchestration {
    public interface IDirtyable {
        event EventHandler<DirtyChangedEventArgs> OnDirtyChanged;
        bool IsDirty { get; }
    }

    public class DirtyChangedEventArgs : EventArgs {
        public readonly bool IsDirty;
        public DirtyChangedEventArgs(bool isDirty) {
            IsDirty = isDirty;
        }
    }
}
