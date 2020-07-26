using QueryWorkbenchUI.Configuration;

namespace QueryWorkbenchUI.ApplicationState {
    public interface IAppStateStore {
        void SaveAppState(string storeKey, AppState appState);
        AppState LoadAppState(string storeKey);
    }
}
