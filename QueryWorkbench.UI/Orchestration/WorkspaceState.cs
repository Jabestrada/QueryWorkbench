namespace QueryWorkbenchUI.Orchestration {
    public enum WorkspaceState {
        Ready,
        ReadyWithError,
        Busy
    }

    public static class WorkspaceStateExtensions {
        public static bool IsReady(this WorkspaceState state) {
            return state == WorkspaceState.Ready || state == WorkspaceState.ReadyWithError;
        }
    }
}
