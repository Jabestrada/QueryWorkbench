using QueryWorkbenchUI.ApplicationState;
using QueryWorkbenchUI.Configuration;
using System;

namespace QueryWorkbench.Tests.Mocks {
    public class MockAppStateStore : IAppStateStore {
        public AppState AppState { get; protected set; }

        public AppState LoadAppState(string storeKey) {
            return new AppState();
        }

        public void SaveAppState(string storeKey, AppState appState) {
            AppState = appState;
        }
    }
}
