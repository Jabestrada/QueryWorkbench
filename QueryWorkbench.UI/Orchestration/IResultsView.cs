using System;

namespace QueryWorkbenchUI.Orchestration {
    public interface IResultsView {
        event EventHandler<ResultsCountChangedArgs> OnResultsCountChanged;
        void ApplyFilter();
        void ToggleOutputPane();
        bool IsOutputPaneVisible { get; set; }
    }

    public class ResultsCountChangedArgs : EventArgs {
        public readonly int OldCount;
        public readonly int NewCount;
        public readonly int ContainerIndex;

        public ResultsCountChangedArgs(int oldCount, int newCount, int containerIndex) {
            OldCount = oldCount;
            NewCount = newCount;
            ContainerIndex = containerIndex;
        }
    }

}
