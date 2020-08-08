namespace QueryWorkbenchUI.Orchestration {
    public interface ITabbedResultsView {
        bool Visible { get; set; }
        void CycleResultsTabForward();
        void CycleResultsTabBackward();
    }
}
